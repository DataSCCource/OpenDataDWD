using DatabaseAccess;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OpenDataDWD
{
    enum AverageData { None, Monthly, Yearly }

    class PlotLogic
    {
        public IDatabaseAccess DataAccess { get; private set; }
        public List<Station> Stations { get; private set; }
        public List<ClimateData> CurrentClimateData { get; private set; }

        private string currentId;
        private AverageData averageData;

        public PlotLogic(IDatabaseAccess dataAccess)
        {
            this.DataAccess = dataAccess;
            if (!DataAccess.StationsDbExists())
            {
                DataAccess.CreateStationsDb();
            }
            this.Stations = DataAccess.LoadStations();

            averageData = AverageData.None;
        }

        public void SetAggregateData(AverageData aggregateData)
        {
            this.averageData = aggregateData;
        }

        public void SetCurrentId(string currentId)
        {
            this.currentId = currentId;
        }

        public void PlotData(Plot myPlot, LineSeries mySeries, DatePicker dateFrom_dp, DatePicker dateTo_dp, string selDataType)
        {
            if (currentId != null)
            {
                Station station = GetCurrentStation();

                // Set filter date the last year of data, if no valid "from" / "to" dates where selected
                var dateTo = dateTo_dp.SelectedDate ?? station.GetDataToDateTime();
                var dateFrom = dateFrom_dp.SelectedDate ?? station.GetDataToDateTime().AddYears(-1);

                // Set displayed dates to data range and limit to selected dates
                // e.g. If "To-Date" is set to 01.01.2020, the "From-Date" after that date is not possible
                dateFrom_dp.DisplayDateStart = station.GetDataFromDateTime();
                dateFrom_dp.DisplayDateEnd = dateTo_dp.SelectedDate;
                dateTo_dp.DisplayDateEnd = station.GetDataToDateTime();
                dateTo_dp.DisplayDateStart = dateFrom_dp.SelectedDate;

                // Get climate data from database
                CurrentClimateData = DataAccess.LoadClimateData(currentId, dateFrom, dateTo).OrderBy(cd => cd.Date).ToList();

                myPlot.Title = station.StationName + $" ({station.GetDataFromDateTime():dd.MM.yyyy} - {station.GetDataToDateTime():dd.MM.yyyy})";
                myPlot.Axes.Clear();

                string selector;
                switch (averageData)
                {
                    case AverageData.None:
                    default:
                        selector = null;
                        break;
                    case AverageData.Monthly:
                        selector = "MMM yyyy";
                        break;
                    case AverageData.Yearly:
                        selector = "yyyy";
                        break;
                }


                IEnumerable<ClimateData> aggregatedClimateData = new List<ClimateData>();
                if (selector == null)
                {
                    aggregatedClimateData = CurrentClimateData;
                }
                else
                {
                    aggregatedClimateData = CurrentClimateData.GroupBy(ccd1 => ccd1.GetDate().ToString(selector)).Select(ccd => new ClimateData
                    {
                        Date = ccd.First().Date,
                        TempMax = ccd.Where(c => c.TempMax > -99).Select(c => c.TempMax).DefaultIfEmpty(-9999).Average(),
                        TempMin = ccd.Where(c => c.TempMin > -99).Select(c => c.TempMin).DefaultIfEmpty(-9999).Average(),
                        PressureMiddle = ccd.Where(c => c.PressureMiddle >= 0).Select(c => c.PressureMiddle).DefaultIfEmpty(-9999).Average(),
                        HumidityMiddle = (int)ccd.Where(c => c.HumidityMiddle > -99).Select(c => c.HumidityMiddle).DefaultIfEmpty(-9999).Average(),
                        WindForceMiddle = (int)ccd.Where(c => c.WindForceMiddle > 0).Select(c => c.WindForceMiddle).DefaultIfEmpty(-9999).Average(),
                        SunshineSum = (int)ccd.Where(c => c.SunshineSum > 0).Select(c => c.SunshineSum).DefaultIfEmpty(-9999).Average(),
                    }).ToList();
                }


                // Get data for selected datatype
                IEnumerable<DataPoint> plotData;
                string unit = " C° ";
                switch (selDataType)
                {
                    case nameof(ClimateData.DataTypes.Temperatur_Tagesmaximum):
                    default:
                        plotData = aggregatedClimateData.Where(c => c.TempMax > -99).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.TempMax, 1)));
                        break;
                    case nameof(ClimateData.DataTypes.Temperatur_Tagesminimum):
                        plotData = aggregatedClimateData.Where(c => c.TempMin > -99).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.TempMin, 1)));
                        break;
                    case nameof(ClimateData.DataTypes.Luftdruck_Tagesmittel):
                        plotData = aggregatedClimateData.Where(c => c.PressureMiddle >= 0).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.PressureMiddle, 1)));
                        unit = " hpa ";
                        break;
                    case nameof(ClimateData.DataTypes.Relative_Luftfeuchte):
                        plotData = aggregatedClimateData.Where(c => c.HumidityMiddle >= 0).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), (double)dat.HumidityMiddle));
                        unit = " % ";
                        break;
                    case nameof(ClimateData.DataTypes.Windstaerke_Tagesmittel):
                        plotData = aggregatedClimateData.Where(c => c.WindForceMiddle >= 0).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.WindForceMiddle, 1)));
                        unit = " Bft ";
                        break;
                    case nameof(ClimateData.DataTypes.Sonnenscheindauer_Tagessumme):
                        plotData = aggregatedClimateData.Where(c => c.SunshineSum > 0).Select(dat => new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dat.GetDate()), Math.Round(dat.SunshineSum, 1)));
                        unit = " h ";
                        break;
                }

                int count = plotData.Count();
                int step = count / 365;
                var yAxis = new OxyPlot.Wpf.LinearAxis() { Position = AxisPosition.Left, Unit = unit + " --> " };
                var xAxis = new OxyPlot.Wpf.DateTimeAxis() { Position = AxisPosition.Bottom };
                myPlot.Axes.Add(yAxis);
                myPlot.Axes.Add(xAxis);

                mySeries.ItemsSource = plotData;
            }
        }

        /// <summary>
        /// Get the currently selected station
        /// </summary>
        /// <returns>currently selected station</returns>
        public Station GetCurrentStation()
        {
            return Stations.Where(st => st.StationKE.Equals(currentId)).First();
        }
    }
}
