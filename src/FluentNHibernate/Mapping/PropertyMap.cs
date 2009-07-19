using System;
using System.Reflection;
using FluentNHibernate.MappingModel;
using NHibernate.UserTypes;

namespace FluentNHibernate.Mapping
{
    public class PropertyMap : IProperty, IAccessStrategy<PropertyMap>
    {
        private readonly Type parentType;
        private readonly AccessStrategyBuilder<PropertyMap> access;
        private readonly PropertyGeneratedBuilder generated;
        private readonly ColumnNameCollection<IProperty> columns;
        private readonly AttributeStore<ColumnMapping> columnAttributes = new AttributeStore<ColumnMapping>();
        private bool nextBool = true;

        private readonly PropertyMapping mapping;

        public PropertyMap(PropertyInfo property, Type parentType)
        {
            columns = new ColumnNameCollection<IProperty>(this);
            access = new AccessStrategyBuilder<PropertyMap>(this, value => mapping.Access = value);
            generated = new PropertyGeneratedBuilder(this, value => mapping.Generated = value);

            this.parentType = parentType;
            mapping = new PropertyMapping
            {
                ContainingEntityType = parentType,
                PropertyInfo = property
            };
        }

        public PropertyGeneratedBuilder Generated
        {
            get { return generated; }
        }

        public PropertyMapping GetPropertyMapping()
        {
            if (columns.List().Count == 0)
                mapping.AddDefaultColumn(CreateColumn(mapping.PropertyInfo.Name));

            foreach (var column in columns.List())
            {
                var columnMapping = CreateColumn(column);

                mapping.AddColumn(columnMapping);
            }

            if (!mapping.IsSpecified(x => x.Name))
                mapping.Name = mapping.PropertyInfo.Name;

            if (!mapping.IsSpecified(x => x.Type))
                mapping.SetDefaultValue(x => x.Type, new TypeReference(mapping.PropertyInfo.PropertyType));

            return mapping;
        }

        private ColumnMapping CreateColumn(string column)
        {
            return new ColumnMapping(columnAttributes.CloneInner())
            {
                Name = column
            };
        }

        public Type PropertyType
        {
            get { return mapping.PropertyInfo.PropertyType; }
        }

        public Type EntityType
        {
            get { return parentType; }
        }

        public IProperty Column(string columnName)
        {
            Columns.Clear();
            Columns.Add(columnName);
            return this;
        }

        public ColumnNameCollection<IProperty> Columns
        {
            get { return columns; }
        }

        IColumnNameCollection IProperty.Columns
        {
            get { return Columns; }
        }

        /// <summary>
        /// Set the access and naming strategy for this property.
        /// </summary>
        public AccessStrategyBuilder<PropertyMap> Access
        {
            get { return access; }
        }

        IAccessStrategyBuilder IProperty.Access
        {
            get { return Access; }
        }

        public IProperty Insert()
        {
            mapping.Insert = nextBool;
            nextBool = true;

            return this;
        }

        public IProperty Update()
        {
            mapping.Update = nextBool;
            nextBool = true;

            return this;
        }

        public IProperty Length(int length)
        {
            columnAttributes.Set(x => x.Length, length);
            return this;
        }

        public IProperty Nullable()
        {
            columnAttributes.Set(x => x.NotNull, !nextBool);
            nextBool = true;
            return this;
        }

        public IProperty ReadOnly()
        {
            mapping.Insert = !nextBool;
            mapping.Update = !nextBool;
            nextBool = true;
            return this;
        }

        public IProperty Formula(string formula) 
        {
            mapping.Formula = formula;
            return this;
        }

        /// <summary>
        /// Specifies that a custom type (an implementation of <see cref="IUserType"/>) should be used for this property for mapping it to/from one or more database columns whose format or type doesn't match this .NET property.
        /// </summary>
        /// <typeparam name="TCustomtype">A type which implements <see cref="IUserType"/>.</typeparam>
        /// <returns>This property mapping to continue the method chain</returns>
        public IProperty CustomType<TCustomtype>()
        {
            return CustomType(typeof(TCustomtype));
        }

        /// <summary>
        /// Specifies that a custom type (an implementation of <see cref="IUserType"/>) should be used for this property for mapping it to/from one or more database columns whose format or type doesn't match this .NET property.
        /// </summary>
        /// <param name="type">A type which implements <see cref="IUserType"/>.</param>
        /// <returns>This property mapping to continue the method chain</returns>
        public IProperty CustomType(Type type)
        {
            if (typeof(ICompositeUserType).IsAssignableFrom(type))
                AddColumnsFromCompositeUserType(type);

            return CustomType(TypeMapping.GetTypeString(type));
        }

        /// <summary>
        /// Specifies that a custom type (an implementation of <see cref="IUserType"/>) should be used for this property for mapping it to/from one or more database columns whose format or type doesn't match this .NET property.
        /// </summary>
        /// <param name="type">A type which implements <see cref="IUserType"/>.</param>
        /// <returns>This property mapping to continue the method chain</returns>
        public IProperty CustomType(string type)
        {
            mapping.Type = new TypeReference(type);

            return this;
        }

        private void AddColumnsFromCompositeUserType(Type compositeUserType)
        {
            var inst = (ICompositeUserType)Activator.CreateInstance(compositeUserType);

            foreach (var name in inst.PropertyNames)
            {
                Columns.Add(name);
            }
        }

        public IProperty CustomSqlType(string sqlType)
        {
            columnAttributes.Set(x => x.SqlType, sqlType);
            return this;
        }

        public IProperty Unique()
        {
            columnAttributes.Set(x => x.Unique, nextBool);
            nextBool = true;
            return this;
        }

        /// <summary>
        /// Specifies the name of a multi-column unique constraint.
        /// </summary>
        /// <param name="keyName">Name of constraint</param>
        public IProperty UniqueKey(string keyName)
        {
            columnAttributes.Set(x => x.UniqueKey, keyName);
            return this;
        }

        public IProperty OptimisticLock()
        {
            mapping.OptimisticLock = nextBool;
            nextBool = true;
            return this;
        }

        /// <summary>
        /// Inverts the next boolean
        /// </summary>
        public IProperty Not
        {
            get
            {
                nextBool = !nextBool;
                return this;
            }
        }
    }
}
