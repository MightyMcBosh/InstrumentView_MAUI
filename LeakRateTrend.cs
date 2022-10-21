using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp; 


namespace VersaMonitor; 

[ObservableObject]
public partial class ViewModel
{
   
    private readonly ObservableCollection<ObservableValue> _observableValues;
    private static readonly int s_logBase = 10; 

    private static  LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint pass = new LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint(
          new[] { new SKColor(0,128,20), new SKColor(10,10,10) },


        // we must go from the point:
        // (x0, y0) where x0 could be read as "the middle of the x axis" (0.5) and y0 as "the start of the y axis" (0)
        new SKPoint(0.5f, 0),

        // to the point:
        // (x1, y1) where x1 could be read as "the middle of the x axis" (0.5) and y0 as "the end of the y axis" (1)
        new SKPoint(0.5f, 1));
    private static  LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint fail = new LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint(
          new[] { new SKColor(128, 0, 20), new SKColor(10, 10, 10) },



        // we must go from the point:
        // (x0, y0) where x0 could be read as "the middle of the x axis" (0.5) and y0 as "the start of the y axis" (0)
        new SKPoint(0.5f, 0),

        // to the point:
        // (x1, y1) where x1 could be read as "the middle of the x axis" (0.5) and y0 as "the end of the y axis" (1)
        new SKPoint(0.5f, 1));
    private static  LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint standby = new LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint(
          new[] { new SKColor(20, 0, 128), new SKColor(10, 10, 10) },

 

        // we must go from the point:
        // (x0, y0) where x0 could be read as "the middle of the x axis" (0.5) and y0 as "the start of the y axis" (0)
        new SKPoint(0.5f, 0),

        // to the point:
        // (x1, y1) where x1 could be read as "the middle of the x axis" (0.5) and y0 as "the end of the y axis" (1)
        new SKPoint(0.5f, 1));

    private LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint _current = standby; 
    public LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint CurrentBackground
    {
        get => _current; 
        set
        {
            if(_current != value)
            {
                _current = value;
                this.OnPropertyChanged(nameof(CurrentBackground));
                
                series.Fill = _current;
                  
            }
        }
    }
    public Axis[] YAxes { get; set; } =
   {
        new Axis
        {
            
            LabelsPaint = new SolidColorPaint(SKColors.White),

            // converts the log scale back for the label
            Labeler = value => string.Format("{0:#E+00}", value), 
        }
    };

    public Axis[] XAxes { get; set; } =
 {
        new Axis
        {
            // forces the step of the axis to be at least 1
            MinStep = 1,
           
            // converts the log scale back for the label
            Labeler = value => value.ToString()
        }
    };

    internal LineSeries<ObservableValue> series;

    public ViewModel()
    {

        
        _current = standby; 

        // Use ObservableCollections to let the chart listen for changes (or any INotifyCollectionChanged). 
        _observableValues = new ObservableCollection<ObservableValue>
        {

        };

        for (int i = 0; i < 60; i++)
        {
            _observableValues.Add(new(0));
        }
        series = new LineSeries<ObservableValue>
        {
            Values = _observableValues,
            Name = "Leak Rate",
            Mapping = (logPoint, chartPoint) =>
            {
                // for the x coordinate, we use the X property of the LogaritmicPoint instance
                chartPoint.SecondaryValue = logPoint.Coordinate.SecondaryValue;

                // but for the Y coordinate, we will map to the logarithm of the value
                chartPoint.PrimaryValue = -Math.Log(logPoint.Coordinate.PrimaryValue, s_logBase);
            },
            DataLabelsSize = 0,
            Stroke = new SolidColorPaint
            {
                Color = new SKColor(255, 255, 255),
                StrokeThickness = 5
            },
            GeometrySize = 0,
            Fill = standby,
        };
        LeakRateSeries = new ObservableCollection<ISeries>
        {
            series,
        };

        
        

        // in the following sample notice that the type int does not implement INotifyPropertyChanged
        // and our Series.Values property is of type List<T>
        // List<T> does not implement INotifyCollectionChanged
        // this means the following series is not listening for changes.
        // Series.Add(new ColumnSeries<int> { Values = new List<int> { 2, 4, 6, 1, 7, -2 } }); 
        InitializeLDMap(); 
    }

    public ObservableCollection<ISeries> LeakRateSeries { get; set; }

    [RelayCommand]
    public void AddItem(double v)
    {
        if (_observableValues.Count >= 60)
            RemoveItem(); 

        _observableValues.Add(new(v));
    }

    [RelayCommand]
    public void RemoveItem()
    {
        if (_observableValues.Count == 0) return;
        _observableValues.RemoveAt(0);
    }


    private static int FastLog10(double input)
    {
        return (input >= 10000000000000) ? 13 :
            (input >= 1000000000000) ? 12 :
            (input >= 100000000000) ? 11 :
            (input >= 10000000000) ? 10 :
            (input >= 1000000000) ? 9 :
            (input >= 100000000) ? 8 :
            (input >= 10000000) ? 7 :
            (input >= 1000000) ? 6 :
            (input >= 100000) ? 5 :
            (input >= 10000) ? 4 :
            (input >= 1000) ? 3 :
            (input >= 100) ? 2 :
            (input >= 10) ? 1 :
            (input >= 1) ? 0 :
            (input >= 0.1) ? -1 :
            (input >= 0.01) ? -2 :
            (input >= 0.001) ? -3 :
            (input >= 0.0001) ? -4 :
            (input >= 0.00001) ? -5 :
            (input >= 0.000001) ? -6 :
            (input >= 0.0000001) ? -7 :
            (input >= 0.00000001) ? -8 :
            (input >= 0.000000001) ? -9 :
            (input >= 0.0000000001) ? -10 :
            (input >= 0.00000000001) ? -11 :
            (input >= 0.000000000001) ? -12 :
            (input >= 0.0000000000001) ? -13 : -14;
    }
}