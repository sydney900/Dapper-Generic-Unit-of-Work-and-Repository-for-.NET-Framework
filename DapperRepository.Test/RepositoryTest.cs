using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Collections.Generic;
using Moq;
using System.Data;
using System.IO;
using System.Linq;
using System.Data.SqlServerCe;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Threading.Tasks;

namespace DapperRepository.Test
{
    [TestClass]
    public class RepositoryTest
    {
        List<TestModelClass> _list;

        public const string FileName = "Test.sdf";
        public const string ConnectionString = "Data Source=./Test.sdf;";
        private IDbConnection GetConnection()
        {
            return new SqlCeConnection(ConnectionString);
        }

        public RepositoryTest()
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }

            var engine = new SqlCeEngine(ConnectionString);
            engine.CreateDatabase();

            SqlMapperExtensions.TableNameMapper = (type) =>
            {
                return type.Name;
            };

            using (var connection = GetConnection())
            {
                connection.Open();
                connection.Execute("CREATE TABLE TestModelClass (Id int IDENTITY(1,1) not null, Name nvarchar(100) not null, Created DateTime null, LastModified DateTime null) ");
            }
        }

        [TestInitialize]
        public void Setup()
        {
            _list = new List<TestModelClass> {
                new TestModelClass { Id=1, Name="Marry"},
                new TestModelClass { Id=2, Name="John"},
                new TestModelClass { Id=3, Name="Doe"}
            };
        }


        [TestCleanup]
        public void TearDown()
        {
            if (_list != null)
                _list.Clear();
        }

        [TestMethod]
        public void CreateRepository_EntityType_ShouldBeToCorrect()
        {
            Repository<TestModelClass> rep = new Repository<TestModelClass>(null);

            rep.EntityType.Name.Should().Be(typeof(TestModelClass).Name);
        }

        private Repository<TestModelClass> PrepareDataAndGetRepository(IDbConnection connection)
        {
            connection.Execute(@"insert TestModelClass (Name, Created, LastModified) values (@Name, @Created, @LastModified)", _list.ToArray());

            Repository<TestModelClass> rep = new Repository<TestModelClass>(connection);
            return rep;
        }

        [TestMethod]
        public void Repository_GetAll_ShouldGetAllRecords()
        {
            using (var connection = GetConnection())
            {
                Repository<TestModelClass> rep = PrepareDataAndGetRepository(connection);

                var queried = rep.GetAll();

                queried.Should().HaveCount(_list.Count);
                queried.Select(x => x.Name).Should().BeEquivalentTo(_list.Select(x => x.Name));
            }
        }

        [TestMethod]
        public async Task Repository_GetAllAsync_ShouldGetAllRecords()
        {
            using (var connection = GetConnection())
            {
                Repository<TestModelClass> rep = PrepareDataAndGetRepository(connection);

                var queried = await rep.GetAllAsync();

                queried.Should().HaveCount(3);
                queried.Select(x => x.Name).Should().BeEquivalentTo(_list.Select(x => x.Name));
            }
        }

        [TestMethod]
        public void Repository_Get_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                Repository<TestModelClass> rep = PrepareDataAndGetRepository(connection);

                long id = 1;
                var queried = rep.Get(id);

                queried.Name.Should().Be(_list.Single(x => x.Id == id).Name);
            }
        }

        [TestMethod]
        public async Task Repository_GetAsync_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                Repository<TestModelClass> rep = PrepareDataAndGetRepository(connection);

                long id = 1;
                var queried = await rep.GetAsync(id);

                queried.Name.Should().Be(_list.Single(x => x.Id == id).Name);
            }
        }

        [TestMethod]
        public void Repository_InsertNull_ShouldThrowException()
        {
            using (var connection = GetConnection())
            {
                Repository<TestModelClass> rep = PrepareDataAndGetRepository(connection);

                Action insert = () => rep.Insert(null);
                insert.Should().Throw<ArgumentNullException>().And.Message.Contains("entity");
            }
        }

        [TestMethod]
        public void Repository_Insert_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                Repository<TestModelClass> rep = PrepareDataAndGetRepository(connection);

                var newOne = new TestModelClass { Id = 4, Name = "Joe" };

                rep.Insert(newOne);

                var queried = rep.GetAll();

                queried.Should().HaveCount(4);
                queried.Last().Name.Should().Be(newOne.Name);
            }
        }

        [TestMethod]
        public void Repository_UpdateNull_ShouldThrowException()
        {
            using (var connection = GetConnection())
            {
                Repository<TestModelClass> rep = PrepareDataAndGetRepository(connection);

                Action update = () => rep.Update(null);
                update.Should().Throw<ArgumentNullException>().And.Message.Contains("entity");
            }
        }

        [TestMethod]
        public void Repository_Update_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                Repository<TestModelClass> rep = PrepareDataAndGetRepository(connection);

                //get a data and change the data
                long id = 1;
                string name = "Steve";
                var queried = rep.Get(id);
                queried.Name = name;

                //then save it to DB
                rep.Update(queried);

                //then get it from DB and check
                queried = rep.Get(id);
                queried.Name.Should().Be(name);
            }
        }

        [TestMethod]
        public void Repository_Delete_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                Repository<TestModelClass> rep = PrepareDataAndGetRepository(connection);

                long id = 1;
                rep.Delete(id);

                var queried = rep.GetAll();

                queried.Should().HaveCount(_list.Count-1);
                queried.FirstOrDefault(x => x.Id == id).Should().BeNull();
            }
        }

        [TestMethod]
        public async Task Repository_DeleteAsync_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                Repository<TestModelClass> rep = PrepareDataAndGetRepository(connection);

                long id = 1;
                await rep.DeleteAsync(id);

                var queried = rep.GetAll();

                queried.Should().HaveCount(_list.Count - 1);
                queried.FirstOrDefault(x => x.Id == id).Should().BeNull();
            }
        }

    }
}
