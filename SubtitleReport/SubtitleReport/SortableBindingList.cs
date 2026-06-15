using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SubtitleReport
{
    public class SortableBindingList<T> : BindingList<T>
    {
        private readonly Dictionary<Type, PropertyComparer<T>> comparers;
        private bool isSorted;
        private ListSortDirection listSortDirection;
        private PropertyDescriptor propertyDescriptor;
        private List<Tuple<string, string>> substituteProperties;


        public SortableBindingList()
            : base(new List<T>())
        {
            this.comparers = new Dictionary<Type, PropertyComparer<T>>();
        }

        public SortableBindingList(IEnumerable<T> enumeration)
            : base(new List<T>(enumeration))
        {
            this.comparers = new Dictionary<Type, PropertyComparer<T>>();
        }


        public void SwitchPropertySort(string sourceProperty, string targetProperty)
        {
            if (string.IsNullOrWhiteSpace(sourceProperty) || string.IsNullOrWhiteSpace(targetProperty))
                throw new ArgumentNullException();

            if (sourceProperty.Equals(targetProperty, StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException();

            var type = typeof(T);
            var piSource = type.GetProperty(sourceProperty);

            if (piSource == null)
                throw new MissingFieldException();

            var piTarget = type.GetProperty(targetProperty);

            if (piTarget == null)
                throw new MissingFieldException();

            if (substituteProperties == null)
                substituteProperties = new List<Tuple<string, string>>();

            substituteProperties.Add(new Tuple<string, string>(sourceProperty, targetProperty));
        }

        protected override bool SupportsSortingCore
        {
            get { return true; }
        }

        protected override bool IsSortedCore
        {
            get { return this.isSorted; }
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get { return this.propertyDescriptor; }
        }

        protected override ListSortDirection SortDirectionCore
        {
            get { return this.listSortDirection; }
        }

        protected override bool SupportsSearchingCore
        {
            get { return true; }
        }

        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
        {
            List<T> itemsList = (List<T>)this.Items;
            PropertyComparer<T> comparer;
            Type propertyType = property.PropertyType;

            var substitute = substituteProperties.FirstOrDefault(f => f.Item1.Equals(property.Name));

            if (substitute != null)
            {
                var substituteProperty = TypeDescriptor.GetProperties(typeof(T)).Find(substitute.Item2, true);
                propertyType = typeof(T).GetProperty(substitute.Item2).GetType();

                if (!this.comparers.TryGetValue(propertyType, out comparer))
                {
                    comparer = new PropertyComparer<T>(substituteProperty, direction);
                    this.comparers.Add(propertyType, comparer);
                }
                comparer.SetPropertyAndDirection(substituteProperty, direction);
            }
            else
            {
                if (!this.comparers.TryGetValue(propertyType, out comparer))
                {
                    comparer = new PropertyComparer<T>(property, direction);
                    this.comparers.Add(propertyType, comparer);
                }
                comparer.SetPropertyAndDirection(property, direction);
            }

            itemsList.Sort(comparer);

            this.propertyDescriptor = property;
            this.listSortDirection = direction;
            this.isSorted = true;

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override void RemoveSortCore()
        {
            this.isSorted = false;
            this.propertyDescriptor = base.SortPropertyCore;
            this.listSortDirection = base.SortDirectionCore;

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override int FindCore(PropertyDescriptor property, object key)
        {
            int count = this.Count;
            for (int i = 0; i < count; ++i)
            {
                T element = this[i];
                if (property.GetValue(element).Equals(key))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}