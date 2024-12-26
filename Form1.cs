using System;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Battery
{
    /// <summary>
    /// Plug in battery parameters such as battery constant voltage, capacity and discharge current to plot discharge curve and terminal voltage.
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ConfigureChart()
        {
   
            ChartArea chartArea = new ChartArea
            {
                Name = "Battery Discharge"
            };

            chartArea.AxisX.Title = "Time (Minutes)";
            chartArea.AxisX.TitleFont = new System.Drawing.Font("Arial", 19, System.Drawing.FontStyle.Bold);
            chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisX.Minimum = 0;
            
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Number;
            chart1.ChartAreas[0].AxisX.IntervalOffset = 1;
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            // Set Y-Axis properties
            chartArea.AxisY.Title = "Voltage (V)";
            chartArea.AxisY.TitleFont = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);

            // Adjust chart area size
            chartArea.Position = new ElementPosition(100, 100, 1, 5);

            chartArea.AxisY.Minimum = 0.7;

            chart1.ChartAreas.Add(chartArea);

            Legend legend = new Legend
            {
                Name = "BatteryLegend",
                Font = new System.Drawing.Font("Arial", 19, System.Drawing.FontStyle.Bold),
                Docking = Docking.Top
            };
            chart1.Legends.Add(legend);

            AddSeries("6.5A Discharge", System.Drawing.Color.Blue);
            AddSeries("13A Discharge", System.Drawing.Color.Green);
            AddSeries("32.5A Discharge", System.Drawing.Color.Red);
        }


        private void AddSeries(string name, System.Drawing.Color color)
        {
            Series series = new Series(name)
            {
                ChartType = SeriesChartType.Line,
                Color = color,
                BorderWidth = 3,
                Legend = "BatteryLegend"

            };
            chart1.Series.Add(series);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="battery"></param>
        /// <param name="dischargeCurrent"></param>
        /// <param name="totalDischargeTime"></param>
        /// <param name="timeStep"></param>
        /// <param name="series"></param>
        private void SimulateRealTimeDischarge(Battery battery, double dischargeCurrent, double timeStep, Series series)
        {
            battery.RemainingCapacity = battery.Capacity;
            double time = 0.0;
            double noLoadVoltage;

            while (battery.RemainingCapacity > 0)
            {
 
                double usedCharge = battery.Capacity - battery.RemainingCapacity; // Qnom - SoC[Ah]

                // Q / (Q - ∫ i dt) term 
                double polarizationEffect = (usedCharge / (battery.Capacity - usedCharge));
                if (double.IsInfinity(polarizationEffect) || double.IsNaN(polarizationEffect))
                {
                    polarizationEffect = 0; // Edge Cases
                }

                // calculate for 'E'
                noLoadVoltage = battery.E0
                                - battery.K * polarizationEffect
                                + battery.A * Math.Exp(-battery.B * usedCharge);

                // Calculate for Vbatt
                double loadVoltage = noLoadVoltage - (dischargeCurrent * battery.R);

                // Assuming cut-off of 0.8, I'mnot sure here tbh...
                if (loadVoltage <= 0.8) break;

                // Reducing SoC
                battery.RemainingCapacity -= dischargeCurrent * timeStep; // Soc

                double minutes = time * 60;
                series.Points.AddXY(minutes, loadVoltage);

                // Increment time
                time += timeStep;
            }
        }





        /// <summary>
        /// 
        /// </summary>
        public class Battery
        {
            public string Type { get; } // Unused
            public double E0 { get; } // No-load voltage
            public double R { get; } // Internal resistance
            public double K { get; } // Polarization voltage
            public double A { get; } // Exponential zone amplitude
            public double B { get; } // Exponential zone coefficient
            public double Capacity { get; } // Battery capacity in Ah
            public double RemainingCapacity { get; set; } // Track remaining capacity

            /// <summary>
            /// Call this with battery params.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="e0"></param>
            /// <param name="r"></param>
            /// <param name="k"></param>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <param name="capacity"></param>
            public Battery(string type, double e0, double r, double k, double a, double b, double capacity)
            {
                Type = type;
                E0 = e0;
                R = r;
                K = k;
                A = a;
                B = b;
                Capacity = capacity;
                RemainingCapacity = capacity;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConfigureChart();

            Battery battery1 = new Battery("Nickel-Metal (6.5A)", 1.28, 0.00046, 0.01875, 0.144, 2.3077, 6.5); // 6.5A
            Battery battery2 = new Battery("Nickel-Metal (13A)", 1.28, 0.00046, 0.01875, 0.144, 2.3077, 6.5); // 13A
            Battery battery3 = new Battery("Nickel-Metal (32.5A)", 1.28, 0.00046, 0.01875, 0.144, 2.3077, 6.5); // 32.5A

            Series series1 = chart1.Series["6.5A Discharge"];
            Series series2 = chart1.Series["13A Discharge"];
            Series series3 = chart1.Series["32.5A Discharge"];

            SimulateRealTimeDischarge(battery1, 6.5, 0.01, series1);
            SimulateRealTimeDischarge(battery2, 13.0, 0.01, series2);
            SimulateRealTimeDischarge(battery3, 32.5, 0.01, series3);
        }
    }
}
