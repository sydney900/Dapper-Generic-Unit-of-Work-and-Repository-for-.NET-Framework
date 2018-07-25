using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;

namespace DapperRepository.Test
{
    [TestClass]
    public class UnitWorkUnitTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateUnitOfWork_WithoutConnectionFactory_ShouldThrowException()
        {
            UnitOfWork unitOfWork = new UnitOfWork(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateUnitOfWork_WithoutRepositories_ShouldThrowException()
        {
            Mock<IConnectionFactory> mockCF = new Mock<IConnectionFactory>();
            UnitOfWork unitOfWork = new UnitOfWork(mockCF.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateUnitOfWork_WithNullRepositories_ShouldThrowException()
        {
            Mock<IConnectionFactory> mockCF = new Mock<IConnectionFactory>();
            UnitOfWork unitOfWork = new UnitOfWork(mockCF.Object, null);
        }

        #region help for following tests
        Mock<IConnectionFactory> mockCF;
        Mock<IDbConnection> mockConnection;
        Mock<Repository<TestModelClass>> mockRepository;
        UnitOfWork unitOfWork;

        private void GetMockUnitOfWork()
        {
            mockCF = new Mock<IConnectionFactory>();
            mockConnection = new Mock<IDbConnection>();
            mockCF.Setup(x => x.GetConnection()).Returns(mockConnection.Object);

            mockRepository = new Mock<Repository<TestModelClass>>(mockConnection.Object);
            unitOfWork = new UnitOfWork(mockCF.Object, mockRepository.Object);
        }        
        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateUnitOfWork_WithOneNullRepository_ShouldThrowException()
        {
            GetMockUnitOfWork();

            UnitOfWork unitOfWork = new UnitOfWork(mockCF.Object, mockRepository.Object, null);
        }

        [TestMethod]
        public void CreateUnitOfWork_WithRepository_ShouldCallRepositorySetConnection()
        {
            GetMockUnitOfWork();

            mockRepository.Verify(x => x.SetConnection(It.IsAny<IDbConnection>()), Times.AtLeastOnce());                        
        }

        [TestMethod]
        public void CreateUnitOfWork_WithAnyClassRepository_ShouldGetThisTypeOfRepository()
        {
            GetMockUnitOfWork();

            Assert.IsInstanceOfType(unitOfWork.Repository<TestModelClass>(), typeof(Repository<TestModelClass>));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UnitOfWork_BeginTransaction_MoreThanOnceBeenCallShouldThrowException()
        {
            GetMockUnitOfWork();
            Mock<IDbTransaction> mockTransaction = new Mock<IDbTransaction>();
            mockConnection.Setup(x => x.BeginTransaction()).Returns(mockTransaction.Object);

            unitOfWork.BeginTransaction();
            unitOfWork.BeginTransaction();
        }

        [TestMethod]
        public void UnitOfWork_BeginTransaction_ShouldCallDbConnectionBeginTransactionMethod()
        {
            GetMockUnitOfWork();
            Mock<IDbTransaction> mockTransaction = new Mock<IDbTransaction>();
            mockConnection.Setup(x => x.BeginTransaction()).Returns(mockTransaction.Object);

            unitOfWork.BeginTransaction();

            mockConnection.Verify(m => m.BeginTransaction());
        }

        [TestMethod]
        public void UnitOfWork_Commit_ShouldCallDbConnectionCommit()
        {
            GetMockUnitOfWork();

            Mock<IDbTransaction> mockTransaction = new Mock<IDbTransaction>();
            mockConnection.Setup(x => x.BeginTransaction()).Returns(mockTransaction.Object);

            unitOfWork.BeginTransaction();
            unitOfWork.Commit();

            mockTransaction.Verify(t => t.Commit());
        }

        public void UnitOfWork_Commit_NotBeginTransactionShouldNotCallDbConnectionCommit()
        {
            GetMockUnitOfWork();

            Mock<IDbTransaction> mockTransaction = new Mock<IDbTransaction>();
            mockConnection.Setup(x => x.BeginTransaction()).Returns(mockTransaction.Object);

            unitOfWork.Commit();

            mockTransaction.Verify(t => t.Commit(), Times.Never());
        }


        [TestMethod]
        public void UnitOfWork_Rollback_ShouldCallDbConnectionRollback()
        {
            GetMockUnitOfWork();

            Mock<IDbTransaction> mockTransaction = new Mock<IDbTransaction>();
            mockConnection.Setup(x => x.BeginTransaction()).Returns(mockTransaction.Object);

            unitOfWork.BeginTransaction();
            unitOfWork.Rollback();

            mockTransaction.Verify(t => t.Rollback());
        }

        [TestMethod]
        public void UnitOfWork_Rollback_NotBeginTransactionShouldNotCallDbConnectionRollback()
        {
            GetMockUnitOfWork();

            Mock<IDbTransaction> mockTransaction = new Mock<IDbTransaction>();
            mockConnection.Setup(x => x.BeginTransaction()).Returns(mockTransaction.Object);

            unitOfWork.Rollback();

            mockTransaction.Verify(t => t.Rollback(), Times.Never());
        }

        [TestMethod]
        public void UnitOfWork_Dispose_ShouldCallTransactionDisposeIfThereIs()
        {
            GetMockUnitOfWork();

            Mock<IDbTransaction> mockTransaction = new Mock<IDbTransaction>();
            mockConnection.Setup(x => x.BeginTransaction()).Returns(mockTransaction.Object);

            unitOfWork.BeginTransaction();
            unitOfWork.Rollback();
            unitOfWork.Dispose();

            mockTransaction.Verify(t => t.Dispose());
        }

        [TestMethod]
        public void UnitOfWork_Dispose_ShouldCallConnectionCloseAndDisposeWhenThereIsTransaction()
        {
            GetMockUnitOfWork();

            Mock<IDbTransaction> mockTransaction = new Mock<IDbTransaction>();
            mockConnection.Setup(x => x.BeginTransaction()).Returns(mockTransaction.Object);

            unitOfWork.Dispose();

            mockConnection.Verify(t => t.Close());
            mockConnection.Verify(t => t.Dispose());
        }

        [TestMethod]
        public void UnitOfWork_Dispose_ShouldCallConnectionCloseAndDisposeWhenThereIsNoTransaction()
        {
            GetMockUnitOfWork();

            unitOfWork.Dispose();

            mockConnection.Verify(t => t.Close());
            mockConnection.Verify(t => t.Dispose());
        }

        [TestMethod]
        public void UnitOfWork_Dispose_ShouldClearAllRepositoried()
        {
            GetMockUnitOfWork();

            unitOfWork.Dispose();

            var clientRep = unitOfWork.Repository<TestModelClass>();
            Assert.IsNull(clientRep);
        }
    }
}
