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
    public class GenericRepositoryTest
    {
        public const string FileName = "Test2.sdf";
        public const string ConnectionString = "Data Source=./Test2.sdf;";

        private static DbConfig GetDbConfig()
        {
            DbConfig config = new DbConfig
            {
                ConnectionString = ConnectionString,
                ProviderName = "System.Data.SqlServerCe.4.0"
            };
            return config;
        }


        List<TestModelClass> _list;

        private IConnectionFactory _connectionFactoryTest;
        private IConnectionFactory ConnectionFactoryInTest
        {
            get
            {
                if (_connectionFactoryTest == null)
                {
                    DbConfig config = GetDbConfig();

                    _connectionFactoryTest = new ConnectionFactory(config);
                }

                return _connectionFactoryTest;
            }
        }

        public GenericRepositoryTest()
        {
            SetupTableMapper();
        }

        private static void SetupTableMapper()
        {
            SqlMapperExtensions.TableNameMapper = (type) =>
            {
                return type.Name;
            };
        }

       
        private void InitializeDb()
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }

            var ceEngine = new SqlCeEngine(ConnectionString);
            ceEngine.CreateDatabase();

            using (var connection = ConnectionFactoryInTest.GetConnection())
            {
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

            InitializeDb();
        }


        [TestCleanup]
        public void TearDown()
        {
            if (_list != null)
                _list.Clear();
        }

        [TestMethod]
        public void CreateGenericRepository_EntityType_ShouldBeToCorrect()
        {
            using(GenericRepository<TestModelClass> rep = new GenericRepository<TestModelClass>(null))
            {
                rep.EntityType.Name.Should().Be(typeof(TestModelClass).Name);
            }
        }

        private GenericRepository<TestModelClass> PrepareDataAndGetRepository()
        {
            using(var connection = ConnectionFactoryInTest.GetConnection())
            {
                connection.Execute(@"insert TestModelClass (Name, Created, LastModified) values (@Name, @Created, @LastModified)", _list.ToArray());
            }            

            GenericRepository<TestModelClass> rep = new GenericRepository<TestModelClass>(ConnectionFactoryInTest);
            return rep;
        }

        [TestMethod]
        public void GenericRepository_GetAll_ShouldGetAllRecords()
        {
            using (GenericRepository<TestModelClass> rep = PrepareDataAndGetRepository())
            {
                var queried = rep.GetAll();

                queried.Should().HaveCount(_list.Count);
                queried.Select(x => x.Name).Should().BeEquivalentTo(_list.Select(x => x.Name));
            }
        }

        [TestMethod]
        public async Task GenericRepository_GetAllAsync_ShouldGetAllRecords()
        {
            using (GenericRepository<TestModelClass> rep = PrepareDataAndGetRepository())
            {
                var queried = await rep.GetAllAsync();

                queried.Should().HaveCount(3);
                queried.Select(x => x.Name).Should().BeEquivalentTo(_list.Select(x => x.Name));
            }
        }

        [TestMethod]
        public void GenericRepository_Get_ShouldWork()
        {
            using (GenericRepository<TestModelClass> rep = PrepareDataAndGetRepository())
            {
                long id = 1;
                var queried = rep.Get(id);

                queried.Name.Should().Be(_list.Single(x => x.Id == id).Name);
            }
        }

        [TestMethod]
        public async Task GenericRepository_GetAsync_ShouldWork()
        {
            using (GenericRepository<TestModelClass> rep = PrepareDataAndGetRepository())
            {
                long id = 1;
                var queried = await rep.GetAsync(id);

                queried.Name.Should().Be(_list.Single(x => x.Id == id).Name);
            }
        }

        [TestMethod]
        public void GenericRepository_InsertNull_ShouldThrowException()
        {
            using (GenericRepository<TestModelClass> rep = PrepareDataAndGetRepository())
            {
                Action insert = () => rep.Insert(null);
                insert.Should().Throw<ArgumentNullException>().And.Message.Contains("entity");
            }
        }

        [TestMethod]
        public void GenericRepository_Insert_ShouldWork()
        {
            using (GenericRepository<TestModelClass> rep = PrepareDataAndGetRepository())
            {
                var newOne = new TestModelClass { Id = 4, Name = "Joe" };

                rep.Insert(newOne);

                var queried = rep.GetAll();

                queried.Should().HaveCount(4);
                queried.Last().Name.Should().Be(newOne.Name);
            }
        }

        [TestMethod]
        public void GenericRepository_UpdateNull_ShouldThrowException()
        {
            using (GenericRepository<TestModelClass> rep = PrepareDataAndGetRepository())
            {
                Action update = () => rep.Update(null);
                update.Should().Throw<ArgumentNullException>().And.Message.Contains("entity");
            }
        }

        [TestMethod]
        public void GenericRepositoryy_Update_ShouldWork()
        {
            using (GenericRepository<TestModelClass> rep = PrepareDataAndGetRepository())
            {
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
        public void GenericRepository_Delete_ShouldWork()
        {
            using (GenericRepository<TestModelClass> rep = PrepareDataAndGetRepository())
            {
                long id = 1;
                rep.Delete(id);

                var queried = rep.GetAll();

                queried.Should().HaveCount(_list.Count - 1);
                queried.FirstOrDefault(x => x.Id == id).Should().BeNull();
            }
        }

        [TestMethod]
        public async Task GenericRepository_DeleteAsync_ShouldWork()
        {
            using (GenericRepository<TestModelClass> rep = PrepareDataAndGetRepository())
            {
                long id = 1;
                await rep.DeleteAsync(id);

                var queried = rep.GetAll();

                queried.Should().HaveCount(_list.Count - 1);
                queried.FirstOrDefault(x => x.Id == id).Should().BeNull();
            }
        }

    }
}
