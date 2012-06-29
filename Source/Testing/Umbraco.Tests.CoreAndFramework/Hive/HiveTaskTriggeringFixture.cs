using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.Exceptions;
using NUnit.Framework;
using Umbraco.Cms.Web.Security.Permissions;
using Umbraco.Cms.Web.System;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security;
using Umbraco.Framework.Tasks;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Hive.Tasks;
using Umbraco.Tests.Extensions;
using Umbraco.Hive;

namespace Umbraco.Tests.CoreAndFramework.Hive
{
    using Umbraco.Cms.Web;
    using Umbraco.Cms.Web.DependencyManagement;
    using Umbraco.Cms.Web.Tasks;
    using Umbraco.Framework.Persistence;
    using Umbraco.Framework.Persistence.Model.Associations;
    using Umbraco.Framework.Persistence.Model.Associations._Revised;
    using Umbraco.Framework.Persistence.Model.Constants.Schemas;
    using Umbraco.Framework.Persistence.Model.Versioning;
    using Umbraco.Hive.Configuration;

    [TestFixture]
    public class HiveTaskTriggeringFixture
    {
        private FakeFrameworkContext _frameworkContext;
        private GroupUnitFactory _groupUnitFactory;
        private AbstractScopedCache _unitScopedCache;

        [SetUp]
        public void Setup()
        {
            _frameworkContext = new FakeFrameworkContext();
            _unitScopedCache = new DictionaryScopedCache();
            var providerGroup = GroupedProviderMockHelper.GenerateProviderGroup(1, 0, 1, _frameworkContext);
            var idRoot = new Uri("oh-yeah://this-is-my-root/");
            _groupUnitFactory = new GroupUnitFactory(providerGroup.Writers, idRoot, FakeHiveCmsManager.CreateFakeRepositoryContext(_frameworkContext), _frameworkContext, () => _unitScopedCache);
        }

        [Test]
        public void WhenEntityIsAdded_WhenCacheWatcherTaskIsRegistered_EntityIsPutInScopedCache()
        {
            // Arrange
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PostAddOrUpdateOnUnitComplete, () => new CacheWatcherTask(_frameworkContext), true);

            // Act
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Cause the task to be fired
                uow.Repositories.AddOrUpdate(anything);
                uow.Complete();

                // Assert the task has been fired
                Assert.That(_unitScopedCache.GetOrCreate(anything.Id.ToString(), () => null), Is.Not.Null);
            }
        }

        [Test]
        public void WhenEntityIsRetrievedViaGet_WhenCacheWatcherTaskIsRegistered_EntityIsPutInScopedCache()
        {
            // Arrange
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PostReadEntity, () => new CacheWatcherTask(_frameworkContext), true);

            // Act
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                uow.Repositories.AddOrUpdate(anything);
                uow.Complete();
            }

            using (var uow = _groupUnitFactory.Create())
            {
                // Cause the task to be fired
                var getItem = uow.Repositories.Get(HiveId.Empty); // Store is mocked
                Assert.NotNull(getItem);

                // Assert the task has been fired
                Assert.That(_unitScopedCache.GetOrCreate(getItem.Id.ToString(), () => null), Is.Not.Null);
            }
        }

        [Test]
        public void WhenRelationIsAdded_ViaAddRelationMethod_PreAndPostRelationAddedBeforeUnitComplete_TasksAreTriggered()
        {
            // Arrange
            var preEventMock = Substitute.For<AbstractTask>(_frameworkContext);
            var postEventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Relations.PreRelationAdded, () => preEventMock, true);
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Relations.PostRelationAdded, () => postEventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var parentAnything = HiveModelCreationHelper.MockTypedEntity();
                var childAnything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => preEventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
                DoesNotThrow(() => postEventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Add items
                uow.Repositories.AddOrUpdate(parentAnything);
                uow.Repositories.AddOrUpdate(childAnything);

                // Add relation and cause the task to be fired
                uow.Repositories.AddRelation(parentAnything, childAnything, FixedRelationTypes.DefaultRelationType, 0);

                // Assert the task has been fired once for the group and once for the repository (handler can check source if they only want to act once)
                DoesNotThrow(() => preEventMock.Received(2).Execute(Arg.Any<TaskExecutionContext>()), "Pre-Call was not received 2 times");
                DoesNotThrow(() => postEventMock.Received(2).Execute(Arg.Any<TaskExecutionContext>()), "Post-Call was not received 2 times");
            }
        }

        private void DoesNotThrow(NUnit.Framework.TestDelegate action, string message = null)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Assert.Fail(message.IfNullOrWhiteSpace(string.Empty) + "\n" + ex.Message + "\n" + ex.InnerException.IfNotNull(x => x.ToString()));
            }
        }

        [Test]
        public void WhenRelationIsRemoved_ViaRemoveRelationMethod_PreAndPostRelationRemovedBeforeUnitComplete_TasksAreTriggered()
        {
            // Arrange
            var preEventMock = Substitute.For<AbstractTask>(_frameworkContext);
            var postEventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Relations.PreRelationRemoved, () => preEventMock, true);
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Relations.PostRelationRemoved, () => postEventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var parentAnything = HiveModelCreationHelper.MockTypedEntity();
                var childAnything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => preEventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
                DoesNotThrow(() => postEventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Add items
                uow.Repositories.AddOrUpdate(parentAnything);
                uow.Repositories.AddOrUpdate(childAnything);

                // Add relation
                uow.Repositories.AddRelation(parentAnything, childAnything, FixedRelationTypes.DefaultRelationType, 0);

                // Remove relation and cause task to be fired
                uow.Repositories.RemoveRelation(new Relation(FixedRelationTypes.DefaultRelationType, parentAnything, childAnything));

                // Assert the task has been fired
                DoesNotThrow(() => preEventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Pre-Call was not received 1 times");
                DoesNotThrow(() => postEventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Post-Call was not received 1 times");
            }
        }

        [Test]
        public void WhenRelationIsAdded_ViaRelationProxies_PreAndPostRelationAddedBeforeUnitComplete_TasksAreTriggered()
        {
            // Arrange
            var preEventMock = Substitute.For<AbstractTask>(_frameworkContext);
            var postEventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Relations.PreRelationAdded, () => preEventMock, true);
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Relations.PostRelationAdded, () => postEventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var parentAnything = HiveModelCreationHelper.MockTypedEntity();
                var childAnything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => preEventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
                DoesNotThrow(() => postEventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Add items
                parentAnything.RelationProxies.EnlistChild(childAnything, FixedRelationTypes.DefaultRelationType);
                uow.Repositories.AddOrUpdate(parentAnything);
                uow.Repositories.AddOrUpdate(childAnything);

                // Complete the uow which should raise the task as relation proxies aren't added until uow completion
                uow.Complete();

                // Assert the task has been fired
                DoesNotThrow(() => preEventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Pre-Call was not received 1 times");
                DoesNotThrow(() => postEventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Post-Call was not received 1 times");
            }
        }

        [Test]
        public void WhenEntityIsDeleted_PreDeleteBeforeUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PreDelete, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.Delete<TypedEntity>(HiveId.Empty);

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received 1 times");
            }
        }

        [Test]
        public void WhenEntityIsDeleted_PostDeleteBeforeUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PostDelete, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.Delete<TypedEntity>(HiveId.Empty);

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received 1 times");
            }
        }

        [Test]
        public void WhenEntityIsDeleted_PreAddOrUpdateAfterUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PreDeleteOnUnitComplete, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.Delete<TypedEntity>(HiveId.Empty);

                // Check the task has still not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Complete the unit
                uow.Complete();

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received");
            }
        }

        [Test]
        public void WhenEntityIsDeleted_PostAddOrUpdateAfterUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PostDeleteOnUnitComplete, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.Delete<TypedEntity>(HiveId.Empty);

                // Check the task has still not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Complete the unit
                uow.Complete();

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received");
            }
        }

        [Test]
        public void WhenEntityIsAdded_PreAddOrUpdateBeforeUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PreAddOrUpdateOnUnitComplete, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.AddOrUpdate(anything);
                uow.Complete();

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received 1 times");
            }
        }

        [Test]
        public void WhenEntityIsAdded_PreAddOrUpdateAfterUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PreAddOrUpdateOnUnitComplete, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.AddOrUpdate(anything);

                // Check the task has still not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Complete the unit
                uow.Complete();

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received");
            }
        }

        [Test]
        public void WhenEntityIsAdded_AndUnitCompletionTaskCausesCancellation_UnitIsRolledBack()
        {
            // Arrange
            var preAddMock = Substitute.For<AbstractTask>(_frameworkContext);
            preAddMock.When(x => x.Execute(Arg.Any<TaskExecutionContext>())).Do(x => x.Arg<TaskExecutionContext>().Cancel = true);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PreAddOrUpdateOnUnitComplete, () => preAddMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => preAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.AddOrUpdate(anything);

                // Check the task has still not yet been fired
                DoesNotThrow(() => preAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Complete the unit
                Assert.That(uow.WasAbandoned, Is.False);
                uow.Complete();

                // Assert
                // Check the task has been fired
                DoesNotThrow(() => preAddMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Task was not executed");
                // Check the unit was then rolled back
                Assert.That(uow.WasAbandoned, Is.True);
            }
        }

        [Test]
        public void WhenEntityIsDeleted_AndUnitCompletionTaskCausesCancellation_UnitIsRolledBack()
        {
            // Arrange
            var preAddMock = Substitute.For<AbstractTask>(_frameworkContext);
            preAddMock.When(x => x.Execute(Arg.Any<TaskExecutionContext>())).Do(x => x.Arg<TaskExecutionContext>().Cancel = true);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PreDeleteOnUnitComplete, () => preAddMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => preAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.Delete<TypedEntity>(HiveId.Empty);

                // Check the task has still not yet been fired
                DoesNotThrow(() => preAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Complete the unit
                Assert.That(uow.WasAbandoned, Is.False);
                uow.Complete();

                // Assert
                // Check the task has been fired
                DoesNotThrow(() => preAddMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Task was not executed");
                // Check the unit was then rolled back
                Assert.That(uow.WasAbandoned, Is.True);
            }
        }

        [Test]
        public void WhenEntityIsAdded_PostAddOrUpdateAfterUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PostAddOrUpdateOnUnitComplete, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.AddOrUpdate(anything);

                // Check the task has still not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Complete the unit
                uow.Complete();

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received 1 times");
            }
        }

        [Test]
        public void WhenEntityRevisionIsAdded_AndUnitCompletionTaskCausesCancellation_UnitIsRolledBack()
        {
            // Arrange
            var preAddMock = Substitute.For<AbstractTask>(_frameworkContext);
            preAddMock.When(x => x.Execute(Arg.Any<TaskExecutionContext>())).Do(x => x.Arg<TaskExecutionContext>().Cancel = true);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Revisions.PreAddOrUpdateOnUnitComplete, () => preAddMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = new Revision<TypedEntity>(HiveModelCreationHelper.MockTypedEntity());

                // Check the task has not yet been fired
                DoesNotThrow(() => preAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.Revisions.AddOrUpdate(anything);

                // Check the task has still not yet been fired
                DoesNotThrow(() => preAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Complete the unit
                Assert.That(uow.WasAbandoned, Is.False);
                uow.Complete();

                // Assert
                // Check the task has been fired
                DoesNotThrow(() => preAddMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Task was not executed");
                // Check the unit was then rolled back
                Assert.That(uow.WasAbandoned, Is.True);
            }
        }

        [Test]
        public void WhenEntityRevisionIsAdded_PreAddOrUpdateAfterUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Revisions.PreAddOrUpdateOnUnitComplete, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = new Revision<TypedEntity>(HiveModelCreationHelper.MockTypedEntity());

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.Revisions.AddOrUpdate(anything);

                // Check the task has still not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Complete the unit
                uow.Complete();

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received");
            }
        }

        [Test]
        public void WhenEntityRevisionIsAdded_PostAddOrUpdateAfterUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Revisions.PostAddOrUpdateOnUnitComplete, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = new Revision<TypedEntity>(HiveModelCreationHelper.MockTypedEntity());

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.Revisions.AddOrUpdate(anything);

                // Check the task has still not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Complete the unit
                uow.Complete();

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received 1 times");
            }
        }

        [Test]
        public void WhenEntityRevisionIsAdded_AndTaskCausesCancellation_EntityIsNotAdded()
        {
            // Arrange
            var preAddMock = Substitute.For<AbstractTask>(_frameworkContext);
            preAddMock.When(x => x.Execute(Arg.Any<TaskExecutionContext>())).Do(x => x.Arg<TaskExecutionContext>().Cancel = true);
            var postAddMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Revisions.PreAddOrUpdate, () => preAddMock, true);
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Revisions.PostAddOrUpdate, () => postAddMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = new Revision<TypedEntity>(HiveModelCreationHelper.MockTypedEntity());

                // Check the pre-add and post-add tasks have not yet been fired
                DoesNotThrow(() => preAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
                DoesNotThrow(() => postAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the pre-add task to be fired
                uow.Repositories.Revisions.AddOrUpdate(anything);

                // Assert the pre-add task has been fired, and the post-add task is not fired
                DoesNotThrow(() => preAddMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received 1 times");
                DoesNotThrow(() => postAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
            }
        }

        [Test]
        public void WhenEntityIsDeleted_AndTaskCausesCancellation_EntityIsNotDeleted()
        {
            // Arrange
            var preMock = Substitute.For<AbstractTask>(_frameworkContext);
            preMock.When(x => x.Execute(Arg.Any<TaskExecutionContext>())).Do(x => x.Arg<TaskExecutionContext>().Cancel = true);
            var postMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PreDelete, () => preMock, true);
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PostDelete, () => postMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = new Revision<TypedEntity>(HiveModelCreationHelper.MockTypedEntity());

                // Check the pre-delete and post-delete tasks have not yet been fired
                DoesNotThrow(() => preMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
                DoesNotThrow(() => postMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the pre-delete task to be fired
                uow.Repositories.Delete<TypedEntity>(HiveId.Empty);

                // Assert the pre-delete task has been fired, and the post-delete task is not fired
                DoesNotThrow(() => preMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received 1 times");
                DoesNotThrow(() => postMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
            }
        }

        [Test]
        public void WhenEntityRevisionIsAdded_PostAddOrUpdateBeforeUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Revisions.PostAddOrUpdate, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = new Revision<TypedEntity>(HiveModelCreationHelper.MockTypedEntity());

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()), "Call was received early");

                // Cause the task to be fired
                uow.Repositories.Revisions.AddOrUpdate(anything);

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received");
            }
        }

        [Test]
        public void WhenEntityRevisionIsAdded_PreAddOrUpdateBeforeUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Revisions.PreAddOrUpdate, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = new Revision<TypedEntity>(HiveModelCreationHelper.MockTypedEntity());

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()), "Call was received early");

                // Cause the task to be fired
                uow.Repositories.Revisions.AddOrUpdate(anything);

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received");
            }
        }

        [Test]
        public void WhenEntityIsAdded_AndTaskCausesCancellation_EntityIsNotAdded()
        {
            // Arrange
            var preAddMock = Substitute.For<AbstractTask>(_frameworkContext);
            preAddMock.When(x => x.Execute(Arg.Any<TaskExecutionContext>())).Do(x => x.Arg<TaskExecutionContext>().Cancel = true);
            var postAddMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PreAddOrUpdate, () => preAddMock, true);
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PostAddOrUpdate, () => postAddMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the pre-add and post-add tasks have not yet been fired
                DoesNotThrow(() => preAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
                DoesNotThrow(() => postAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the pre-add task to be fired
                uow.Repositories.AddOrUpdate(anything);

                // Assert the pre-add task has been fired, and the post-add task is not fired
                DoesNotThrow(() => preAddMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received 1 times");
                DoesNotThrow(() => postAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
            }
        }

        [Test]
        public void WhenEntityIsAdded_PostAddOrUpdateBeforeUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PostAddOrUpdate, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()), "Call was received early");

                // Cause the task to be fired
                uow.Repositories.AddOrUpdate(anything);

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received");
            }
        }

        private void SetupContentTest(IHiveManager hive)
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent schema and content root exists for this test
            var contentVirtualRoot = FixedEntities.ContentVirtualRoot;
            var systemRoot = new SystemRoot();
            var contentRootSchema = new ContentRootSchema();
            hive.AutoCommitTo<IContentStore>(
                x =>
                {
                    x.Repositories.AddOrUpdate(systemRoot);
                    x.Repositories.AddOrUpdate(contentVirtualRoot);
                    x.Repositories.Schemas.AddOrUpdate(contentRootSchema);
                });
        }

        [Test]
        public void Integration_WhenEntityIsAdded_EnsureUniqueNameTask_RenamesItemIfNotUnique()
        {
            IHiveManager hive = null;
            try
            {
                var nhibernateTestSetup = new NhibernateTestSetupHelper(useNhProf:true);

                var storageProvider = new IoHiveTestSetupHelper(nhibernateTestSetup.FakeFrameworkContext);

                hive = new HiveManager(
                        new[]
                        {
                            new ProviderMappingGroup(
                                "test",
                                new WildcardUriMatch("content://"),
                                nhibernateTestSetup.ReadonlyProviderSetup,
                                nhibernateTestSetup.ProviderSetup,
                                nhibernateTestSetup.FakeFrameworkContext),
                            storageProvider.CreateGroup("uploader", "storage://file-uploader"),
                        },
                        nhibernateTestSetup.FakeFrameworkContext);


                SetupContentTest(hive);

                hive.FrameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Revisions.PreAddOrUpdateOnUnitComplete,
                                          () => new EnsureUniqueNameTask(hive.FrameworkContext),
                                          true);

                hive.Cms().NewContentType("test-for-dupes")
                    .Define("sometext", "singleLineTextBox", "tab1")
                    .Commit();

                var parent = hive.Cms().NewRevision("My site root", "root", "test-for-dupes")
                    .SetValue("sometext", "hallow")
                    .Publish()
                    .Commit();

                var firstItem = hive.Cms().NewRevision("My Name", "my-name", "test-for-dupes")
                    .SetValue("sometext", "hallow")
                    .SetParent(parent.Item)
                    .Publish()
                    .Commit();

                var firstUnDupedItem = hive.Cms().NewRevision("My Name Unpublished", "my-name-unpublished", "test-for-dupes")
                    .SetValue("sometext", "hallow")
                    .SetParent(parent.Item)
                    .Publish()
                    .Commit();

                var shouldBeModified = hive.Cms().NewRevision("My Name", "my-name", "test-for-dupes")
                    .SetValue("sometext", "hallow")
                    .SetParent(parent.Item)
                    .Publish()
                    .Commit();

                var shouldBeModified2 = hive.Cms().NewRevision("My Name", "my-name", "test-for-dupes")
                    .SetValue("sometext", "hallow")
                    .SetParent(parent.Item)
                    .Publish()
                    .Commit();

                Assert.That(shouldBeModified.Success, Is.True);
                Assert.That(firstItem.Item.Id, Is.Not.EqualTo(shouldBeModified.Item.Id));
                Assert.That(firstItem.Item.Id, Is.Not.EqualTo(shouldBeModified2.Item.Id));
                Assert.That(shouldBeModified.Item.Id, Is.Not.EqualTo(shouldBeModified2.Item.Id));

                hive.Context.GenerationScopedCache.Clear();

                var queryForName = hive.Cms().QueryContent().Where(x => x.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, "Name") == "My Name").ToList();
                Assert.That(queryForName.Count, Is.EqualTo(1));

                hive.Context.GenerationScopedCache.Clear();

                using(var uow = hive.OpenReader<IContentStore>())
                {
                    var checkFirstItem = uow.Repositories.Get(firstItem.Item.Id);
                    var checkRenamedItem = uow.Repositories.Get(shouldBeModified.Item.Id);
                    var checkRenamedItem2 = uow.Repositories.Get(shouldBeModified2.Item.Id);

                    var firstItemName = checkFirstItem.Attributes[NodeNameAttributeDefinition.AliasValue].Values["Name"];
                    var checkRenamedItemName = checkRenamedItem.Attributes[NodeNameAttributeDefinition.AliasValue].Values["Name"];
                    var checkRenamedItemName2 = checkRenamedItem2.Attributes[NodeNameAttributeDefinition.AliasValue].Values["Name"];

                    Assert.That(firstItemName, Is.Not.EqualTo(checkRenamedItemName));
                    Assert.That(firstItemName, Is.Not.EqualTo(checkRenamedItemName2));
                    Assert.That(checkRenamedItemName, Is.Not.EqualTo(checkRenamedItemName2));
                    Assert.That(checkRenamedItemName, Is.EqualTo("My Name (1)"));
                    Assert.That(checkRenamedItemName2, Is.EqualTo("My Name (2)"));

                    var firstItemUrlName = checkFirstItem.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"];
                    var checkRenamedItemUrlName = checkRenamedItem.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"];
                    var checkRenamedItemUrlName2 = checkRenamedItem2.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"];

                    Assert.That(firstItemUrlName, Is.Not.EqualTo(checkRenamedItemUrlName));
                    Assert.That(firstItemUrlName, Is.Not.EqualTo(checkRenamedItemUrlName2));
                    Assert.That(checkRenamedItemUrlName, Is.Not.EqualTo(checkRenamedItemUrlName2));
                    Assert.That(checkRenamedItemUrlName, Is.EqualTo("my-name-1"));
                    Assert.That(checkRenamedItemUrlName2, Is.EqualTo("my-name-2"));
                }
            }
            finally
            {
                hive.IfNotNull(x => x.Dispose());
            }
            
        }

        [Test]
        public void WhenEntityIsAdded_PreAddOrUpdateBeforeUnitComplete_TaskChangesItem_ItemIsSavedWithNewValues()
        {
            // Arrange
            var nhibernateTestSetup = new NhibernateTestSetupHelper();

            var storageProvider = new IoHiveTestSetupHelper(nhibernateTestSetup.FakeFrameworkContext);

            var hive = new HiveManager(
                    new[]
                        {
                            new ProviderMappingGroup(
                                "test",
                                new WildcardUriMatch("content://"),
                                nhibernateTestSetup.ReadonlyProviderSetup,
                                nhibernateTestSetup.ProviderSetup,
                                nhibernateTestSetup.FakeFrameworkContext),
                            storageProvider.CreateGroup("uploader", "storage://file-uploader"),
                        },
                    nhibernateTestSetup.FakeFrameworkContext);

            var eventMock = Substitute.For<AbstractTask>(hive.FrameworkContext);
            eventMock.When(x => x.Execute(Arg.Any<TaskExecutionContext>()))
                .Do(x =>
            {
                var context = x.Args()[0] as TaskExecutionContext;
                if(context != null)
                {
                    var uow = context.EventSource as IGroupUnit<IProviderTypeFilter>;
                    var args = context.EventArgs.CallerEventArgs as HiveEntityPreActionEventArgs;
                    if (uow != null && args != null)
                    {
                        var item = args.Entity as TypedEntity;
                        if (item != null &&
                            item.Attributes.Any(
                                y => y.AttributeDefinition.Alias == NodeNameAttributeDefinition.AliasValue))
                        {
                            item.Attributes[NodeNameAttributeDefinition.AliasValue].Values["Name"] = "Something else";

                            uow.Repositories.AddOrUpdate(item);
                        }
                    }
                }
            });

            // Act
            hive.FrameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PreAddOrUpdateOnUnitComplete, () => eventMock, true);

            var id = HiveId.Empty;

            using (var uow = hive.OpenWriter<IContentStore>())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Cause the task to be fired
                uow.Repositories.AddOrUpdate(anything);
                uow.Complete();

                id = anything.Id;
            }

            using (var uow = hive.OpenWriter<IContentStore>())
            {
                // Cause the task to be fired
                var getItem = uow.Repositories.Get(id); // Store is mocked

                Assert.NotNull(getItem);

                // Assert the task has been fired
                Assert.AreEqual("Something else", getItem.Attributes[NodeNameAttributeDefinition.AliasValue].Values["Name"]);
            }
        }
    }
}