﻿//      *********    DO NOT MODIFY THIS FILE     *********
//      This file is regenerated by a design tool. Making
//      changes to this file can cause errors.
namespace Expression.Blend.SampleData.BoardDesignData
{
	using System; 

// To significantly reduce the sample data footprint in your production application, you can set
// the DISABLE_SAMPLE_DATA conditional compilation constant and disable sample data at runtime.
#if DISABLE_SAMPLE_DATA
	internal class BoardDesignData { }
#else

	public class BoardDesignData : System.ComponentModel.INotifyPropertyChanged
	{
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}

		public BoardDesignData()
		{
			try
			{
				System.Uri resourceUri = new System.Uri("/Sinobyl.WPF;component/SampleData/BoardDesignData/BoardDesignData.xaml", System.UriKind.Relative);
				if (System.Windows.Application.GetResourceStream(resourceUri) != null)
				{
					System.Windows.Application.LoadComponent(this, resourceUri);
				}
			}
			catch (System.Exception)
			{
			}
		}

		private Squares _Squares = new Squares();

		public Squares Squares
		{
			get
			{
				return this._Squares;
			}
		}
	}

	public class SquaresItem : System.ComponentModel.INotifyPropertyChanged
	{
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}

		private string _Name = string.Empty;

		public string Name
		{
			get
			{
				return this._Name;
			}

			set
			{
				if (this._Name != value)
				{
					this._Name = value;
					this.OnPropertyChanged("Name");
				}
			}
		}

		private bool _IsLight = false;

		public bool IsLight
		{
			get
			{
				return this._IsLight;
			}

			set
			{
				if (this._IsLight != value)
				{
					this._IsLight = value;
					this.OnPropertyChanged("IsLight");
				}
			}
		}

		private double _RowIndex = 0;

		public double RowIndex
		{
			get
			{
				return this._RowIndex;
			}

			set
			{
				if (this._RowIndex != value)
				{
					this._RowIndex = value;
					this.OnPropertyChanged("RowIndex");
				}
			}
		}

		private double _ColIndex = 0;

		public double ColIndex
		{
			get
			{
				return this._ColIndex;
			}

			set
			{
				if (this._ColIndex != value)
				{
					this._ColIndex = value;
					this.OnPropertyChanged("ColIndex");
				}
			}
		}
	}

	public class Squares : System.Collections.ObjectModel.ObservableCollection<SquaresItem>
	{ 
	}
#endif
}