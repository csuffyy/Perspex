﻿// -----------------------------------------------------------------------
// <copyright file="PerspexObjectTests.cs" company="Steven Kirk">
// Copyright 2013 MIT Licence. See licence.md for more information.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Perspex.Base.UnitTests
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PerspexObjectTests
    {
        [TestInitialize]
        public void Initialize()
        {
            // Ensure properties are registered.
            PerspexProperty p;
            p = Class1.FooProperty;
            p = Class2.BarProperty;
        }

        [TestMethod]
        public void GetProperties_Returns_Registered_Properties()
        {
            string[] names = PerspexObject.GetProperties(typeof(Class1)).Select(x => x.Name).ToArray();

            CollectionAssert.AreEqual(new[] { "Foo", "Baz", "Qux" }, names);
        }

        [TestMethod]
        public void GetProperties_Returns_Registered_Properties_For_Base_Types()
        {
            string[] names = PerspexObject.GetProperties(typeof(Class2)).Select(x => x.Name).ToArray();

            CollectionAssert.AreEqual(new[] { "Bar", "Foo", "Baz", "Qux" }, names);
        }

        [TestMethod]
        public void GetValue_Returns_Default_Value()
        {
            Class1 target = new Class1();

            Assert.AreEqual("foodefault", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void GetValue_Returns_Overridden_Default_Value()
        {
            Class2 target = new Class2();

            Assert.AreEqual("foooverride", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void GetValue_Returns_Set_Value()
        {
            Class1 target = new Class1();

            target.SetValue(Class1.FooProperty, "newvalue");

            Assert.AreEqual("newvalue", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void GetValue_Returns_Inherited_Value()
        {
            Class1 parent = new Class1();
            Class2 child = new Class2 { Parent = parent };

            parent.SetValue(Class1.BazProperty, "changed");

            Assert.AreEqual("changed", child.GetValue(Class1.BazProperty));
        }

        [TestMethod]
        public void ClearValue_Clears_Value()
        {
            Class1 target = new Class1();

            target.SetValue(Class1.FooProperty, "newvalue");
            target.ClearValue(Class1.FooProperty);

            Assert.AreEqual("foodefault", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void SetValue_Sets_Value()
        {
            Class1 target = new Class1();

            target.SetValue(Class1.FooProperty, "newvalue");

            Assert.AreEqual("newvalue", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void SetValue_Raises_PropertyChanged()
        {
            Class1 target = new Class1();
            bool raised = false;

            target.PropertyChanged += (s, e) =>
            {
                raised = s == target &&
                         e.Property == Class1.FooProperty &&
                         (string)e.OldValue == "foodefault" &&
                         (string)e.NewValue == "newvalue";
            };

            target.SetValue(Class1.FooProperty, "newvalue");

            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void SetValue_Doesnt_Raise_PropertyChanged_If_Value_Not_Changed()
        {
            Class1 target = new Class1();
            bool raised = false;

            target.SetValue(Class1.FooProperty, "bar");

            target.PropertyChanged += (s, e) =>
            {
                raised = true;
            };

            target.SetValue(Class1.FooProperty, "bar");

            Assert.IsFalse(raised);
        }

        [TestMethod]
        public void SetValue_Doesnt_Raise_PropertyChanged_If_Value_Not_Changed_From_Default()
        {
            Class1 target = new Class1();
            bool raised = false;

            target.PropertyChanged += (s, e) =>
            {
                raised = true;
            };

            target.SetValue(Class1.FooProperty, "foodefault");

            Assert.IsFalse(raised);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetValue_Throws_Exception_For_Unregistered_Property()
        {
            Class1 target = new Class1();

            target.SetValue(Class2.BarProperty, "invalid");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetValue_Throws_Exception_For_Invalid_Value_Type()
        {
            Class1 target = new Class1();

            target.SetValue(Class1.FooProperty, 123);
        }

        [TestMethod]
        public void SetValue_Causes_Coercion()
        {
            Class1 target = new Class1();

            target.SetValue(Class1.QuxProperty, 5);
            Assert.AreEqual(5, target.GetValue(Class1.QuxProperty));
            target.SetValue(Class1.QuxProperty, -5);
            Assert.AreEqual(0, target.GetValue(Class1.QuxProperty));
            target.SetValue(Class1.QuxProperty, 15);
            Assert.AreEqual(10, target.GetValue(Class1.QuxProperty));
        }

        [TestMethod]
        public void CoerceValue_Causes_Recoercion()
        {
            Class1 target = new Class1();

            target.SetValue(Class1.QuxProperty, 7);
            Assert.AreEqual(7, target.GetValue(Class1.QuxProperty));
            target.MaxQux = 5;
            target.CoerceValue(Class1.QuxProperty);
        }

        [TestMethod]
        public void GetObservable_Returns_Initial_Value()
        {
            Class1 target = new Class1();
            bool raised = false;

            target.GetObservable(Class1.FooProperty).Subscribe(x => raised = x == "foodefault");

            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void GetObservable_Returns_Property_Change()
        {
            Class1 target = new Class1();
            bool raised = false;

            target.GetObservable(Class1.FooProperty).Subscribe(x => raised = x == "newvalue");
            raised = false;
            target.SetValue(Class1.FooProperty, "newvalue");

            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void GetObservable_Returns_Property_Change_Only_For_Correct_Property()
        {
            Class2 target = new Class2();
            bool raised = false;

            target.GetObservable(Class1.FooProperty).Subscribe(x => raised = true);
            raised = false;
            target.SetValue(Class2.BarProperty, "newvalue");

            Assert.IsFalse(raised);
        }

        [TestMethod]
        public void GetObservable_Dispose_Stops_Property_Changes()
        {
            Class1 target = new Class1();
            bool raised = false;

            target.GetObservable(Class1.FooProperty)
                  .Subscribe(x => raised = true)
                  .Dispose();
            raised = false;
            target.SetValue(Class1.FooProperty, "newvalue");

            Assert.IsFalse(raised);
        }

        [TestMethod]
        public void Setting_InheritanceParent_Raises_PropertyChanged_When_Value_Changed_In_Parent()
        {
            bool raised = false;

            Class1 parent = new Class1();
            parent.SetValue(Class1.BazProperty, "changed");

            Class2 child = new Class2();
            child.PropertyChanged += (s, e) =>
                raised = s == child &&
                         e.Property == Class1.BazProperty &&
                         (string)e.OldValue == "bazdefault" &&
                         (string)e.NewValue == "changed";

            child.Parent = parent;

            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void Setting_InheritanceParent_Doesnt_Raise_PropertyChanged_When_Local_Value_Set()
        {
            bool raised = false;

            Class1 parent = new Class1();
            parent.SetValue(Class1.BazProperty, "changed");

            Class2 child = new Class2();
            child.SetValue(Class1.BazProperty, "localvalue");
            child.PropertyChanged += (s, e) => raised = true;

            child.Parent = parent;

            Assert.IsFalse(raised);
        }

        [TestMethod]
        public void Setting_Value_In_InheritanceParent_Raises_PropertyChanged()
        {
            bool raised = false;

            Class1 parent = new Class1();

            Class2 child = new Class2();
            child.PropertyChanged += (s, e) =>
                raised = s == child &&
                         e.Property == Class1.BazProperty &&
                         (string)e.OldValue == "bazdefault" &&
                         (string)e.NewValue == "changed";
            child.Parent = parent;

            parent.SetValue(Class1.BazProperty, "changed");

            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void Bind_Sets_Current_Value()
        {
            Class1 target = new Class1();
            Class1 source = new Class1();

            source.SetValue(Class1.FooProperty, "initial");
            target.Bind(Class1.FooProperty, source.GetObservable(Class1.FooProperty));

            Assert.AreEqual("initial", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void Bind_NonGeneric_Sets_Current_Value()
        {
            Class1 target = new Class1();
            Class1 source = new Class1();

            source.SetValue(Class1.FooProperty, "initial");
            target.Bind((PerspexProperty)Class1.FooProperty, source.GetObservable(Class1.FooProperty));

            Assert.AreEqual("initial", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void Bind_Sets_Subsequent_Value()
        {
            Class1 target = new Class1();
            Class1 source = new Class1();

            source.SetValue(Class1.FooProperty, "initial");
            target.Bind(Class1.FooProperty, source.GetObservable(Class1.FooProperty));
            source.SetValue(Class1.FooProperty, "subsequent");

            Assert.AreEqual("subsequent", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void Binding_Doesnt_Set_Value_After_Clear()
        {
            Class1 target = new Class1();
            Class1 source = new Class1();

            source.SetValue(Class1.FooProperty, "initial");
            target.Bind(Class1.FooProperty, source.GetObservable(Class1.FooProperty));
            target.ClearValue(Class1.FooProperty);
            source.SetValue(Class1.FooProperty, "newvalue");

            Assert.AreEqual("foodefault", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void Bind_Doesnt_Set_Value_After_Reset()
        {
            Class1 target = new Class1();
            Class1 source = new Class1();

            source.SetValue(Class1.FooProperty, "initial");
            target.Bind(Class1.FooProperty, source.GetObservable(Class1.FooProperty));
            target.SetValue(Class1.FooProperty, "reset");
            source.SetValue(Class1.FooProperty, "newvalue");

            Assert.AreEqual("reset", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Bind_Throws_Exception_For_Invalid_Value_Type()
        {
            Class1 target = new Class1();

            target.Bind((PerspexProperty)Class1.FooProperty, Observable.Return((object)123));
        }

        [TestMethod]
        public void BindTwoWay_Gets_Initial_Value_From_Source()
        {
            Class1 source = new Class1();
            Class1 target = new Class1();

            source.SetValue(Class1.FooProperty, "initial");
            target.BindTwoWay(Class1.FooProperty, source, Class1.FooProperty);

            Assert.AreEqual("initial", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void BindTwoWay_Updates_Values()
        {
            Class1 source = new Class1();
            Class1 target = new Class1();

            source.SetValue(Class1.FooProperty, "first");
            target.BindTwoWay(Class1.FooProperty, source, Class1.FooProperty);

            Assert.AreEqual("first", target.GetValue(Class1.FooProperty));
            source.SetValue(Class1.FooProperty, "second");
            Assert.AreEqual("second", target.GetValue(Class1.FooProperty));
            target.SetValue(Class1.FooProperty, "third");
            Assert.AreEqual("third", source.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void Setting_UnsetValue_Reverts_To_Default_Value()
        {
            Class1 target = new Class1();

            target.SetValue(Class1.FooProperty, "newvalue");
            target.SetValue(Class1.FooProperty, PerspexProperty.UnsetValue);

            Assert.AreEqual("foodefault", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void StyleBinding_Overrides_Default_Value()
        {
            Class1 target = new Class1();

            target.Bind(Class1.FooProperty, this.Single("stylevalue"), BindingPriority.Style);

            Assert.AreEqual("stylevalue", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void StyleBinding_Doesnt_Override_Local_Value()
        {
            Class1 target = new Class1();

            target.SetValue(Class1.FooProperty, "newvalue");
            target.Bind(Class1.FooProperty, this.Single("stylevalue"), BindingPriority.Style);

            Assert.AreEqual("newvalue", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void this_Operator_Returns_Value_Property()
        {
            Class1 target = new Class1();

            target.SetValue(Class1.FooProperty, "newvalue");

            Assert.AreEqual("newvalue", target[Class1.FooProperty]);
        }

        [TestMethod]
        public void this_Operator_Sets_Value_Property()
        {
            Class1 target = new Class1();

            target[Class1.FooProperty] = "newvalue";

            Assert.AreEqual("newvalue", target.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void this_Operator_Doesnt_Accept_Observable()
        {
            Class1 target = new Class1();

            target[Class1.FooProperty] = Observable.Return("newvalue");
        }

        [TestMethod]
        public void this_Operator_Binds_One_Way()
        {
            Class1 target1 = new Class1();
            Class1 target2 = new Class1();
            Binding binding = Class1.FooProperty.Bind().WithMode(BindingMode.OneWay);

            target1.SetValue(Class1.FooProperty, "first");
            target2[binding] = target1[!Class1.FooProperty];
            target1.SetValue(Class1.FooProperty, "second");

            Assert.AreEqual("second", target2.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void this_Operator_Binds_Two_Way()
        {
            Class1 target1 = new Class1();
            Class1 target2 = new Class1();
            Binding binding = Class1.FooProperty.Bind().WithMode(BindingMode.TwoWay);

            target1.SetValue(Class1.FooProperty, "first");
            target2[binding] = target1[!Class1.FooProperty];
            Assert.AreEqual("first", target2.GetValue(Class1.FooProperty));
            target1.SetValue(Class1.FooProperty, "second");
            Assert.AreEqual("second", target2.GetValue(Class1.FooProperty));
            target2.SetValue(Class1.FooProperty, "third");
            Assert.AreEqual("third", target1.GetValue(Class1.FooProperty));
        }

        [TestMethod]
        public void this_Operator_Binds_One_Time()
        {
            Class1 target1 = new Class1();
            Class1 target2 = new Class1();
            Binding binding = Class1.FooProperty.Bind().WithMode(BindingMode.OneTime);

            target1.SetValue(Class1.FooProperty, "first");
            target2[binding] = target1[!Class1.FooProperty];
            target1.SetValue(Class1.FooProperty, "second");

            Assert.AreEqual("first", target2.GetValue(Class1.FooProperty));
        }

        /// <summary>
        /// Returns an observable that returns a single value but does not complete.
        /// </summary>
        /// <typeparam name="T">The type of the observable.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The observable.</returns>
        private IObservable<T> Single<T>(T value)
        {
            return Observable.Never<T>().StartWith(value);
        }

        private class Class1 : PerspexObject
        {
            public static readonly PerspexProperty<string> FooProperty =
                PerspexProperty.Register<Class1, string>("Foo", "foodefault");

            public static readonly PerspexProperty<string> BazProperty =
                PerspexProperty.Register<Class1, string>("Baz", "bazdefault", true);

            public static readonly PerspexProperty<int> QuxProperty =
                PerspexProperty.Register<Class1, int>("Qux", coerce: Coerce);

            public int MaxQux { get; set; }

            public Class1()
            {
                this.MaxQux = 10;
            }

            private static int Coerce(PerspexObject instance, int value)
            {
                return Math.Min(Math.Max(value, 0), ((Class1)instance).MaxQux);
            }
        }

        private class Class2 : Class1
        {
            public static readonly PerspexProperty<string> BarProperty =
                PerspexProperty.Register<Class2, string>("Bar", "bardefault");

            static Class2()
            {
                FooProperty.OverrideDefaultValue(typeof(Class2), "foooverride");
            }

            public Class1 Parent
            {
                get { return (Class1)this.InheritanceParent; }
                set { this.InheritanceParent = value; }
            }
        }
    }
}
