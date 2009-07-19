﻿using System.Linq;
using FluentNHibernate.Conventions.Instances;
using FluentNHibernate.MappingModel;
using NUnit.Framework;

namespace FluentNHibernate.Testing.ConventionsTests.Alterations
{
    [TestFixture, Category("Alteration DSL")]
    public class PropertyAlterationMapsToPropertyMapping
    {
        private PropertyMapping mapping;
        private IPropertyInstance alteration;

        [SetUp]
        public void CreateDsl()
        {
            mapping = new PropertyMapping();
            alteration = new PropertyInstance(mapping);
        }

        [Test]
        public void AccessMapped()
        {
            alteration.Access.Field();
            mapping.Access.ShouldEqual("field");
        }

        [Test]
        public void ColumnShouldInheritAttributesFromDefault()
        {
            mapping.AddDefaultColumn(new ColumnMapping { NotNull = true });

            alteration.Column("test");

            mapping.Columns.Count().ShouldEqual(1);
            mapping.Columns.First().Name.ShouldEqual("test");
            mapping.Columns.First().NotNull.ShouldBeTrue(); ;
        }

        [Test]
        public void ColumnShouldHaveAttributesAppliedWhenAddedBeforeSetting()
        {
            mapping.AddDefaultColumn(new ColumnMapping());

            alteration.Column("test");
            alteration.Not.Nullable();

            mapping.Columns.Count().ShouldEqual(1);
            mapping.Columns.First().Name.ShouldEqual("test");
            mapping.Columns.First().NotNull.ShouldBeTrue(); ;
        }

        [Test]
        public void ColumnShouldHaveAttributesAppliedWhenAddedAfterSetting()
        {
            mapping.AddDefaultColumn(new ColumnMapping());

            alteration.Not.Nullable();
            alteration.Column("test");

            mapping.Columns.Count().ShouldEqual(1);
            mapping.Columns.First().Name.ShouldEqual("test");
            mapping.Columns.First().NotNull.ShouldBeTrue(); ;
        }
    }
}