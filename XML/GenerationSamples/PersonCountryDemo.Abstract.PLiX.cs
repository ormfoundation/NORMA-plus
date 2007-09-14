﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
namespace PersonCountryDemo
{
	#region Person
	[DataObject()]
	[System.CodeDom.Compiler.GeneratedCode("OIALtoPLiX", "1.0")]
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
	public abstract partial class Person : INotifyPropertyChanged, IHasPersonCountryDemoContext
	{
		protected Person()
		{
		}
		#region Person INotifyPropertyChanged Implementation
		private PropertyChangedEventHandler _propertyChangedEventHandler;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add
			{
				if ((object)value != null)
				{
					PropertyChangedEventHandler currentHandler;
					while ((object)System.Threading.Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this._propertyChangedEventHandler, (PropertyChangedEventHandler)System.Delegate.Combine(currentHandler = this._propertyChangedEventHandler, value), currentHandler) != (object)currentHandler)
					{
					}
				}
			}
			remove
			{
				if ((object)value != null)
				{
					PropertyChangedEventHandler currentHandler;
					while ((object)System.Threading.Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this._propertyChangedEventHandler, (PropertyChangedEventHandler)System.Delegate.Remove(currentHandler = this._propertyChangedEventHandler, value), currentHandler) != (object)currentHandler)
					{
					}
				}
			}
		}
		private void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler eventHandler;
			if ((object)(eventHandler = this._propertyChangedEventHandler) != null)
			{
				EventHandlerUtility.InvokeEventHandlerAsync(eventHandler, this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion // Person INotifyPropertyChanged Implementation
		#region Person Property Change Events
		private System.Delegate[] _events;
		private System.Delegate[] Events
		{
			get
			{
				System.Delegate[] localEvents;
				return (localEvents = this._events) ?? (System.Threading.Interlocked.CompareExchange<System.Delegate[]>(ref this._events, localEvents = new System.Delegate[10], null) ?? localEvents);
			}
		}
		private static void InterlockedDelegateCombine(ref System.Delegate location, System.Delegate value)
		{
			System.Delegate currentHandler;
			while ((object)System.Threading.Interlocked.CompareExchange<System.Delegate>(ref location, System.Delegate.Combine(currentHandler = location, value), currentHandler) != (object)currentHandler)
			{
			}
		}
		private static void InterlockedDelegateRemove(ref System.Delegate location, System.Delegate value)
		{
			System.Delegate currentHandler;
			while ((object)System.Threading.Interlocked.CompareExchange<System.Delegate>(ref location, System.Delegate.Remove(currentHandler = location, value), currentHandler) != (object)currentHandler)
			{
			}
		}
		public event EventHandler<PropertyChangingEventArgs<Person, int>> Person_idChanging
		{
			add
			{
				if ((object)value != null)
				{
					Person.InterlockedDelegateCombine(ref this.Events[0], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Person.InterlockedDelegateRemove(ref events[0], value);
				}
			}
		}
		protected bool OnPerson_idChanging(int newValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangingEventArgs<Person, int>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangingEventArgs<Person, int>>)events[0]) != null))
			{
				return EventHandlerUtility.InvokeCancelableEventHandler<PropertyChangingEventArgs<Person, int>>(eventHandler, this, new PropertyChangingEventArgs<Person, int>(this, "Person_id", this.Person_id, newValue));
			}
			return true;
		}
		public event EventHandler<PropertyChangedEventArgs<Person, int>> Person_idChanged
		{
			add
			{
				if ((object)value != null)
				{
					Person.InterlockedDelegateCombine(ref this.Events[1], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Person.InterlockedDelegateRemove(ref events[1], value);
				}
			}
		}
		protected void OnPerson_idChanged(int oldValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangedEventArgs<Person, int>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangedEventArgs<Person, int>>)events[1]) != null))
			{
				EventHandlerUtility.InvokeEventHandlerAsync<PropertyChangedEventArgs<Person, int>>(eventHandler, this, new PropertyChangedEventArgs<Person, int>(this, "Person_id", oldValue, this.Person_id), this._propertyChangedEventHandler);
			}
			else
			{
				this.OnPropertyChanged("Person_id");
			}
		}
		public event EventHandler<PropertyChangingEventArgs<Person, string>> LastNameChanging
		{
			add
			{
				if ((object)value != null)
				{
					Person.InterlockedDelegateCombine(ref this.Events[2], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Person.InterlockedDelegateRemove(ref events[2], value);
				}
			}
		}
		protected bool OnLastNameChanging(string newValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangingEventArgs<Person, string>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangingEventArgs<Person, string>>)events[2]) != null))
			{
				return EventHandlerUtility.InvokeCancelableEventHandler<PropertyChangingEventArgs<Person, string>>(eventHandler, this, new PropertyChangingEventArgs<Person, string>(this, "LastName", this.LastName, newValue));
			}
			return true;
		}
		public event EventHandler<PropertyChangedEventArgs<Person, string>> LastNameChanged
		{
			add
			{
				if ((object)value != null)
				{
					Person.InterlockedDelegateCombine(ref this.Events[3], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Person.InterlockedDelegateRemove(ref events[3], value);
				}
			}
		}
		protected void OnLastNameChanged(string oldValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangedEventArgs<Person, string>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangedEventArgs<Person, string>>)events[3]) != null))
			{
				EventHandlerUtility.InvokeEventHandlerAsync<PropertyChangedEventArgs<Person, string>>(eventHandler, this, new PropertyChangedEventArgs<Person, string>(this, "LastName", oldValue, this.LastName), this._propertyChangedEventHandler);
			}
			else
			{
				this.OnPropertyChanged("LastName");
			}
		}
		public event EventHandler<PropertyChangingEventArgs<Person, string>> FirstNameChanging
		{
			add
			{
				if ((object)value != null)
				{
					Person.InterlockedDelegateCombine(ref this.Events[4], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Person.InterlockedDelegateRemove(ref events[4], value);
				}
			}
		}
		protected bool OnFirstNameChanging(string newValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangingEventArgs<Person, string>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangingEventArgs<Person, string>>)events[4]) != null))
			{
				return EventHandlerUtility.InvokeCancelableEventHandler<PropertyChangingEventArgs<Person, string>>(eventHandler, this, new PropertyChangingEventArgs<Person, string>(this, "FirstName", this.FirstName, newValue));
			}
			return true;
		}
		public event EventHandler<PropertyChangedEventArgs<Person, string>> FirstNameChanged
		{
			add
			{
				if ((object)value != null)
				{
					Person.InterlockedDelegateCombine(ref this.Events[5], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Person.InterlockedDelegateRemove(ref events[5], value);
				}
			}
		}
		protected void OnFirstNameChanged(string oldValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangedEventArgs<Person, string>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangedEventArgs<Person, string>>)events[5]) != null))
			{
				EventHandlerUtility.InvokeEventHandlerAsync<PropertyChangedEventArgs<Person, string>>(eventHandler, this, new PropertyChangedEventArgs<Person, string>(this, "FirstName", oldValue, this.FirstName), this._propertyChangedEventHandler);
			}
			else
			{
				this.OnPropertyChanged("FirstName");
			}
		}
		public event EventHandler<PropertyChangingEventArgs<Person, string>> TitleChanging
		{
			add
			{
				if ((object)value != null)
				{
					Person.InterlockedDelegateCombine(ref this.Events[6], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Person.InterlockedDelegateRemove(ref events[6], value);
				}
			}
		}
		protected bool OnTitleChanging(string newValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangingEventArgs<Person, string>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangingEventArgs<Person, string>>)events[6]) != null))
			{
				return EventHandlerUtility.InvokeCancelableEventHandler<PropertyChangingEventArgs<Person, string>>(eventHandler, this, new PropertyChangingEventArgs<Person, string>(this, "Title", this.Title, newValue));
			}
			return true;
		}
		public event EventHandler<PropertyChangedEventArgs<Person, string>> TitleChanged
		{
			add
			{
				if ((object)value != null)
				{
					Person.InterlockedDelegateCombine(ref this.Events[7], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Person.InterlockedDelegateRemove(ref events[7], value);
				}
			}
		}
		protected void OnTitleChanged(string oldValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangedEventArgs<Person, string>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangedEventArgs<Person, string>>)events[7]) != null))
			{
				EventHandlerUtility.InvokeEventHandlerAsync<PropertyChangedEventArgs<Person, string>>(eventHandler, this, new PropertyChangedEventArgs<Person, string>(this, "Title", oldValue, this.Title), this._propertyChangedEventHandler);
			}
			else
			{
				this.OnPropertyChanged("Title");
			}
		}
		public event EventHandler<PropertyChangingEventArgs<Person, Country>> CountryChanging
		{
			add
			{
				if ((object)value != null)
				{
					Person.InterlockedDelegateCombine(ref this.Events[8], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Person.InterlockedDelegateRemove(ref events[8], value);
				}
			}
		}
		protected bool OnCountryChanging(Country newValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangingEventArgs<Person, Country>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangingEventArgs<Person, Country>>)events[8]) != null))
			{
				return EventHandlerUtility.InvokeCancelableEventHandler<PropertyChangingEventArgs<Person, Country>>(eventHandler, this, new PropertyChangingEventArgs<Person, Country>(this, "Country", this.Country, newValue));
			}
			return true;
		}
		public event EventHandler<PropertyChangedEventArgs<Person, Country>> CountryChanged
		{
			add
			{
				if ((object)value != null)
				{
					Person.InterlockedDelegateCombine(ref this.Events[9], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Person.InterlockedDelegateRemove(ref events[9], value);
				}
			}
		}
		protected void OnCountryChanged(Country oldValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangedEventArgs<Person, Country>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangedEventArgs<Person, Country>>)events[9]) != null))
			{
				EventHandlerUtility.InvokeEventHandlerAsync<PropertyChangedEventArgs<Person, Country>>(eventHandler, this, new PropertyChangedEventArgs<Person, Country>(this, "Country", oldValue, this.Country), this._propertyChangedEventHandler);
			}
			else
			{
				this.OnPropertyChanged("Country");
			}
		}
		#endregion // Person Property Change Events
		#region Person Abstract Properties
		public abstract PersonCountryDemoContext Context
		{
			get;
		}
		[DataObjectField(false, false, false)]
		public abstract int Person_id
		{
			get;
			set;
		}
		[DataObjectField(false, false, false)]
		public abstract string LastName
		{
			get;
			set;
		}
		[DataObjectField(false, false, false)]
		public abstract string FirstName
		{
			get;
			set;
		}
		[DataObjectField(false, false, true)]
		public abstract string Title
		{
			get;
			set;
		}
		[DataObjectField(false, false, true)]
		public abstract Country Country
		{
			get;
			set;
		}
		#endregion // Person Abstract Properties
		#region Person ToString Methods
		public override string ToString()
		{
			return this.ToString(null);
		}
		public virtual string ToString(IFormatProvider provider)
		{
			return string.Format(provider, @"Person{0}{{{0}{1}Person_id = ""{2}"",{0}{1}LastName = ""{3}"",{0}{1}FirstName = ""{4}"",{0}{1}Title = ""{5}"",{0}{1}Country = {6}{0}}}", Environment.NewLine, @"	", this.Person_id, this.LastName, this.FirstName, this.Title, "TODO: Recursively call ToString for customTypes...");
		}
		#endregion // Person ToString Methods
	}
	#endregion // Person
	#region Country
	[DataObject()]
	[System.CodeDom.Compiler.GeneratedCode("OIALtoPLiX", "1.0")]
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
	public abstract partial class Country : INotifyPropertyChanged, IHasPersonCountryDemoContext
	{
		protected Country()
		{
		}
		#region Country INotifyPropertyChanged Implementation
		private PropertyChangedEventHandler _propertyChangedEventHandler;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add
			{
				if ((object)value != null)
				{
					PropertyChangedEventHandler currentHandler;
					while ((object)System.Threading.Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this._propertyChangedEventHandler, (PropertyChangedEventHandler)System.Delegate.Combine(currentHandler = this._propertyChangedEventHandler, value), currentHandler) != (object)currentHandler)
					{
					}
				}
			}
			remove
			{
				if ((object)value != null)
				{
					PropertyChangedEventHandler currentHandler;
					while ((object)System.Threading.Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this._propertyChangedEventHandler, (PropertyChangedEventHandler)System.Delegate.Remove(currentHandler = this._propertyChangedEventHandler, value), currentHandler) != (object)currentHandler)
					{
					}
				}
			}
		}
		private void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler eventHandler;
			if ((object)(eventHandler = this._propertyChangedEventHandler) != null)
			{
				EventHandlerUtility.InvokeEventHandlerAsync(eventHandler, this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion // Country INotifyPropertyChanged Implementation
		#region Country Property Change Events
		private System.Delegate[] _events;
		private System.Delegate[] Events
		{
			get
			{
				System.Delegate[] localEvents;
				return (localEvents = this._events) ?? (System.Threading.Interlocked.CompareExchange<System.Delegate[]>(ref this._events, localEvents = new System.Delegate[4], null) ?? localEvents);
			}
		}
		private static void InterlockedDelegateCombine(ref System.Delegate location, System.Delegate value)
		{
			System.Delegate currentHandler;
			while ((object)System.Threading.Interlocked.CompareExchange<System.Delegate>(ref location, System.Delegate.Combine(currentHandler = location, value), currentHandler) != (object)currentHandler)
			{
			}
		}
		private static void InterlockedDelegateRemove(ref System.Delegate location, System.Delegate value)
		{
			System.Delegate currentHandler;
			while ((object)System.Threading.Interlocked.CompareExchange<System.Delegate>(ref location, System.Delegate.Remove(currentHandler = location, value), currentHandler) != (object)currentHandler)
			{
			}
		}
		public event EventHandler<PropertyChangingEventArgs<Country, string>> Country_nameChanging
		{
			add
			{
				if ((object)value != null)
				{
					Country.InterlockedDelegateCombine(ref this.Events[0], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Country.InterlockedDelegateRemove(ref events[0], value);
				}
			}
		}
		protected bool OnCountry_nameChanging(string newValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangingEventArgs<Country, string>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangingEventArgs<Country, string>>)events[0]) != null))
			{
				return EventHandlerUtility.InvokeCancelableEventHandler<PropertyChangingEventArgs<Country, string>>(eventHandler, this, new PropertyChangingEventArgs<Country, string>(this, "Country_name", this.Country_name, newValue));
			}
			return true;
		}
		public event EventHandler<PropertyChangedEventArgs<Country, string>> Country_nameChanged
		{
			add
			{
				if ((object)value != null)
				{
					Country.InterlockedDelegateCombine(ref this.Events[1], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Country.InterlockedDelegateRemove(ref events[1], value);
				}
			}
		}
		protected void OnCountry_nameChanged(string oldValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangedEventArgs<Country, string>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangedEventArgs<Country, string>>)events[1]) != null))
			{
				EventHandlerUtility.InvokeEventHandlerAsync<PropertyChangedEventArgs<Country, string>>(eventHandler, this, new PropertyChangedEventArgs<Country, string>(this, "Country_name", oldValue, this.Country_name), this._propertyChangedEventHandler);
			}
			else
			{
				this.OnPropertyChanged("Country_name");
			}
		}
		public event EventHandler<PropertyChangingEventArgs<Country, string>> Region_Region_codeChanging
		{
			add
			{
				if ((object)value != null)
				{
					Country.InterlockedDelegateCombine(ref this.Events[2], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Country.InterlockedDelegateRemove(ref events[2], value);
				}
			}
		}
		protected bool OnRegion_Region_codeChanging(string newValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangingEventArgs<Country, string>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangingEventArgs<Country, string>>)events[2]) != null))
			{
				return EventHandlerUtility.InvokeCancelableEventHandler<PropertyChangingEventArgs<Country, string>>(eventHandler, this, new PropertyChangingEventArgs<Country, string>(this, "Region_Region_code", this.Region_Region_code, newValue));
			}
			return true;
		}
		public event EventHandler<PropertyChangedEventArgs<Country, string>> Region_Region_codeChanged
		{
			add
			{
				if ((object)value != null)
				{
					Country.InterlockedDelegateCombine(ref this.Events[3], value);
				}
			}
			remove
			{
				System.Delegate[] events;
				if (((object)value != null) && ((object)(events = this._events) != null))
				{
					Country.InterlockedDelegateRemove(ref events[3], value);
				}
			}
		}
		protected void OnRegion_Region_codeChanged(string oldValue)
		{
			System.Delegate[] events;
			EventHandler<PropertyChangedEventArgs<Country, string>> eventHandler;
			if (((object)(events = this._events) != null) && ((object)(eventHandler = (EventHandler<PropertyChangedEventArgs<Country, string>>)events[3]) != null))
			{
				EventHandlerUtility.InvokeEventHandlerAsync<PropertyChangedEventArgs<Country, string>>(eventHandler, this, new PropertyChangedEventArgs<Country, string>(this, "Region_Region_code", oldValue, this.Region_Region_code), this._propertyChangedEventHandler);
			}
			else
			{
				this.OnPropertyChanged("Region_Region_code");
			}
		}
		#endregion // Country Property Change Events
		#region Country Abstract Properties
		public abstract PersonCountryDemoContext Context
		{
			get;
		}
		[DataObjectField(false, false, false)]
		public abstract string Country_name
		{
			get;
			set;
		}
		[DataObjectField(false, false, true)]
		public abstract string Region_Region_code
		{
			get;
			set;
		}
		[DataObjectField(false, false, true)]
		public abstract IEnumerable<Person> PersonViaCountryCollection
		{
			get;
		}
		#endregion // Country Abstract Properties
		#region Country ToString Methods
		public override string ToString()
		{
			return this.ToString(null);
		}
		public virtual string ToString(IFormatProvider provider)
		{
			return string.Format(provider, @"Country{0}{{{0}{1}Country_name = ""{2}"",{0}{1}Region_Region_code = ""{3}""{0}}}", Environment.NewLine, @"	", this.Country_name, this.Region_Region_code);
		}
		#endregion // Country ToString Methods
	}
	#endregion // Country
	#region IHasPersonCountryDemoContext
	[System.CodeDom.Compiler.GeneratedCode("OIALtoPLiX", "1.0")]
	public interface IHasPersonCountryDemoContext
	{
		PersonCountryDemoContext Context
		{
			get;
		}
	}
	#endregion // IHasPersonCountryDemoContext
	#region IPersonCountryDemoContext
	[System.CodeDom.Compiler.GeneratedCode("OIALtoPLiX", "1.0")]
	public interface IPersonCountryDemoContext
	{
		Person GetPersonByPerson_id(int Person_id);
		bool TryGetPersonByPerson_id(int Person_id, out Person Person);
		Country GetCountryByCountry_name(string Country_name);
		bool TryGetCountryByCountry_name(string Country_name, out Country Country);
		Person CreatePerson(int Person_id, string LastName, string FirstName);
		IEnumerable<Person> PersonCollection
		{
			get;
		}
		Country CreateCountry(string Country_name);
		IEnumerable<Country> CountryCollection
		{
			get;
		}
	}
	#endregion // IPersonCountryDemoContext
}
