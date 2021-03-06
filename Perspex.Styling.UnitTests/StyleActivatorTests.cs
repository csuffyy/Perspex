﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Perspex.Styling.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Perspex.Styling;

    [TestClass]
    public class StyleActivatorTests
    {
        [TestMethod]
        public void Activator_And_Should_Follow_Single_Input()
        {
            var inputs = new[] { new TestSubject<bool>(false) };
            var target = new StyleActivator(inputs, ActivatorMode.And);
            var result = new TestObserver<bool>();

            target.Subscribe(result);
            Assert.IsFalse(result.GetValue());
            inputs[0].OnNext(true);
            Assert.IsTrue(result.GetValue());
            inputs[0].OnNext(false);
            Assert.IsFalse(result.GetValue());
            inputs[0].OnNext(true);
            Assert.IsTrue(result.GetValue());

            Assert.AreEqual(1, inputs[0].SubscriberCount);
        }

        [TestMethod]
        public void Activator_And_Should_AND_Multiple_Inputs()
        {
            var inputs = new[] 
            { 
                new TestSubject<bool>(false),
                new TestSubject<bool>(false),
                new TestSubject<bool>(true),
            };
            var target = new StyleActivator(inputs, ActivatorMode.And);
            var result = new TestObserver<bool>();

            target.Subscribe(result);
            Assert.IsFalse(result.GetValue());
            inputs[0].OnNext(true);
            inputs[1].OnNext(true);
            Assert.IsTrue(result.GetValue());
            inputs[0].OnNext(false);
            Assert.IsFalse(result.GetValue());

            Assert.AreEqual(1, inputs[0].SubscriberCount);
            Assert.AreEqual(1, inputs[1].SubscriberCount);
            Assert.AreEqual(1, inputs[2].SubscriberCount);
        }

        [TestMethod]
        public void Activator_And_Should_Unsubscribe_All_When_Input_Completes_On_False()
        {
            var inputs = new[] 
            { 
                new TestSubject<bool>(false),
                new TestSubject<bool>(false),
                new TestSubject<bool>(true),
            };
            var target = new StyleActivator(inputs, ActivatorMode.And);
            var result = new TestObserver<bool>();

            target.Subscribe(result);
            Assert.IsFalse(result.GetValue());
            inputs[0].OnNext(true);
            inputs[1].OnNext(true);
            Assert.IsTrue(result.GetValue());
            inputs[0].OnNext(false);
            Assert.IsFalse(result.GetValue());
            inputs[0].OnCompleted();

            Assert.AreEqual(0, inputs[0].SubscriberCount);
            Assert.AreEqual(0, inputs[1].SubscriberCount);
            Assert.AreEqual(0, inputs[2].SubscriberCount);
        }

        [TestMethod]
        public void Activator_And_Should_Not_Unsubscribe_All_When_Input_Completes_On_True()
        {
            var inputs = new[] 
            { 
                new TestSubject<bool>(false),
                new TestSubject<bool>(false),
                new TestSubject<bool>(true),
            };
            var target = new StyleActivator(inputs, ActivatorMode.And);
            var result = new TestObserver<bool>();

            target.Subscribe(result);
            Assert.IsFalse(result.GetValue());
            inputs[0].OnNext(true);
            inputs[0].OnCompleted();

            Assert.AreEqual(1, inputs[0].SubscriberCount);
            Assert.AreEqual(1, inputs[1].SubscriberCount);
            Assert.AreEqual(1, inputs[2].SubscriberCount);
        }

        [TestMethod]
        public void Activator_Or_Should_Follow_Single_Input()
        {
            var inputs = new[] { new TestSubject<bool>(false) };
            var target = new StyleActivator(inputs, ActivatorMode.Or);
            var result = new TestObserver<bool>();

            target.Subscribe(result);
            Assert.IsFalse(result.GetValue());
            inputs[0].OnNext(true);
            Assert.IsTrue(result.GetValue());
            inputs[0].OnNext(false);
            Assert.IsFalse(result.GetValue());
            inputs[0].OnNext(true);
            Assert.IsTrue(result.GetValue());

            Assert.AreEqual(1, inputs[0].SubscriberCount);
        }

        [TestMethod]
        public void Activator_Or_Should_OR_Multiple_Inputs()
        {
            var inputs = new[] 
            { 
                new TestSubject<bool>(false),
                new TestSubject<bool>(false),
                new TestSubject<bool>(true),
            };
            var target = new StyleActivator(inputs, ActivatorMode.Or);
            var result = new TestObserver<bool>();

            target.Subscribe(result);
            Assert.IsTrue(result.GetValue());
            inputs[2].OnNext(false);
            Assert.IsFalse(result.GetValue());
            inputs[0].OnNext(true);
            Assert.IsTrue(result.GetValue());

            Assert.AreEqual(1, inputs[0].SubscriberCount);
            Assert.AreEqual(1, inputs[1].SubscriberCount);
            Assert.AreEqual(1, inputs[2].SubscriberCount);
        }

        [TestMethod]
        public void Activator_Or_Should_Unsubscribe_All_When_Input_Completes_On_True()
        {
            var inputs = new[] 
            { 
                new TestSubject<bool>(false),
                new TestSubject<bool>(false),
                new TestSubject<bool>(true),
            };
            var target = new StyleActivator(inputs, ActivatorMode.Or);
            var result = new TestObserver<bool>();

            target.Subscribe(result);
            Assert.IsTrue(result.GetValue());
            inputs[2].OnNext(false);
            Assert.IsFalse(result.GetValue());
            inputs[0].OnNext(true);
            Assert.IsTrue(result.GetValue());
            inputs[0].OnCompleted();

            Assert.AreEqual(0, inputs[0].SubscriberCount);
            Assert.AreEqual(0, inputs[1].SubscriberCount);
            Assert.AreEqual(0, inputs[2].SubscriberCount);
        }

        [TestMethod]
        public void Activator_Or_Should_Not_Unsubscribe_All_When_Input_Completes_On_False()
        {
            var inputs = new[] 
            { 
                new TestSubject<bool>(false),
                new TestSubject<bool>(false),
                new TestSubject<bool>(true),
            };
            var target = new StyleActivator(inputs, ActivatorMode.Or);
            var result = new TestObserver<bool>();

            target.Subscribe(result);
            Assert.IsTrue(result.GetValue());
            inputs[2].OnNext(false);
            Assert.IsFalse(result.GetValue());
            inputs[2].OnCompleted();

            Assert.AreEqual(1, inputs[0].SubscriberCount);
            Assert.AreEqual(1, inputs[1].SubscriberCount);
            Assert.AreEqual(1, inputs[2].SubscriberCount);
        }

        [TestMethod]
        public void Completed_Activator_Should_Signal_OnCompleted()
        {
            var inputs = new[]
            {
                Observable.Return(false),
            };

            var target = new StyleActivator(inputs, ActivatorMode.Or);
            var completed = false;

            target.Subscribe(_ => { }, () => completed = true);

            Assert.IsTrue(completed);
        }
    }
}
