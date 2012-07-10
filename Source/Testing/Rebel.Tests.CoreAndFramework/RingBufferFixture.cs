using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Tests.CoreAndFramework
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Rebel.Framework;
    using Rebel.Framework.Diagnostics;

    [TestFixture]
    public class RingBufferFixture
    {
        [Test]
        public void PerfTest()
        {
            RingBuffer<string> ringBuffer = new RingBuffer<string>(1000);
            Stopwatch ringWatch = new Stopwatch();
            ringWatch.Start();
            for (int i = 0; i < 1000000; i++)
            {
                ringBuffer.Enqueue("StringOfFun");
            }
            ringWatch.Stop();

            Assert.That(ringWatch.ElapsedMilliseconds, Is.LessThan(100));
        }

        [Test]
        public void PerfTestThreads()
        {
            RingBuffer<string> ringBuffer = new RingBuffer<string>(1000);

            Stopwatch ringWatch = new Stopwatch();
            List<Task> ringTasks = new List<Task>();
            for (int t = 0; t < 10; t++)
            {
                ringTasks.Add(new Task(() =>
                {
                    for (int i = 0; i < 1000000; i++)
                    {
                        ringBuffer.Enqueue("StringOfFun");
                    }
                }));

            }
            ringWatch.Start();
            ringTasks.ForEach(t => t.Start());
            Task.WaitAny(ringTasks.ToArray());
            ringWatch.Stop();

            Assert.That(ringWatch.ElapsedMilliseconds, Is.LessThan(500));
        }

        [Test]
        public void PerfTestThreadsWithDequeues()
        {
            RingBuffer<string> ringBuffer = new RingBuffer<string>(1000);

            Stopwatch ringWatch = new Stopwatch();
            List<Task> ringTasks = new List<Task>();
            for (int t = 0; t < 10; t++)
            {
                ringTasks.Add(new Task(() =>
                {
                    for (int i = 0; i < 1000000; i++)
                    {
                        ringBuffer.Enqueue("StringOfFun");
                    }
                }));

            }
            for (int t = 0; t < 10; t++)
            {
                ringTasks.Add(new Task(() =>
                {
                    for (int i = 0; i < 1000000; i++)
                    {
                        string foo;
                        ringBuffer.TryDequeue(out foo);
                    }
                }));
            }
            ringWatch.Start();
            ringTasks.ForEach(t => t.Start());
            Task.WaitAny(ringTasks.ToArray());
            ringWatch.Stop();

            Assert.That(ringWatch.ElapsedMilliseconds, Is.LessThan(800));
        }

        [Test]
        public void WhenRingSizeLimitIsHit_ItemsAreDequeued()
        {
            // Arrange
            const int limit = 2;
            object object1 = "one";
            object object2 = "two";
            object object3 = "three";
            RingBuffer<object> queue = new RingBuffer<object>(limit);

            // Act
            queue.Enqueue(object1);
            queue.Enqueue(object2);
            queue.Enqueue(object3);

            // Assert
            object value;
            queue.TryDequeue(out value);
            Assert.That(value, Is.EqualTo(object2));
            queue.TryDequeue(out value);
            Assert.That(value, Is.EqualTo(object3));
            queue.TryDequeue(out value);
            Assert.That(value, Is.Null);
        }
    }
}
