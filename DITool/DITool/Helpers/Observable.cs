using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DITool.Helpers
{
    public class Observable<T> : INotifyPropertyChanged, IEquatable<T>
    {
		private T _value;

		public T Value
		{
			get => _value;
			set
			{
				if (_value.Equals(value))
					return;

				_value = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
			}
		}

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Equals(T other)
			=> _value.Equals(other);


		public Observable(T value)
			=> _value = value;
    }
}
