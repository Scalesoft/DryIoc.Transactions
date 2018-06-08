using Castle.Facilities.AutoTx.Testing;
using Castle.Facilities.AutoTx.Tests.TestClasses;
using DryIoc;
using DryIoc.Facilities.AutoTx.Extensions;
using NUnit.Framework;

namespace Castle.Facilities.AutoTx.Tests
{
	public class SingleThread_SupressInAmbient
	{
		private Container _Container;

		[SetUp]
		public void SetUp()
		{
			_Container = new Container();
			_Container.AddFacility<AutoTxFacility>();
			_Container.Register<MyService>(Reuse.Singleton);
		}

		[TearDown]
		public void TearDown()
		{
			_Container.Dispose();
		}

		[Test]
		public void SupressedTransaction_NoCurrentTransaction()
		{
			using (var scope = new ResolveScope<MyService>(_Container))
				scope.Service.VerifySupressed();
		}
		[Test]
		public void SupressedTransaction_InCurrentTransaction()
		{
			using (var scope = new ResolveScope<MyService>(_Container))
				scope.Service.VerifyInAmbient(() => scope.Service.VerifySupressed());
		}
	}

    public class InheritanceTransaction
    {
        private Container _Container;

        [SetUp]
        public void SetUp()
        {
            _Container = new Container();
            _Container.AddFacility<AutoTxFacility>();
            _Container.Register<InheritedMyService>(Reuse.Singleton);
        }

        [TearDown]
        public void TearDown()
        {
            _Container.Dispose();
        }


        [Test]
        public void InheritanceKeepTransaction()
        {
            using (var scope = new ResolveScope<InheritedMyService>(_Container))
            {
                scope.Service.VerifyInAmbient();
            }
        }
    }
}