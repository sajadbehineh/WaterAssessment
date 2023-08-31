using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WaterAssessment.Converters;

namespace WaterAssessment.Views;

public sealed partial class AssessmentFormPage : Page
{
    public List<Employee> Employees { get; set; } = new List<Employee>();
    public List<Assessment_Employee> AssessmentEmployees { get; set; } = new();
    public List<Location> Locations { get; set; } = new List<Location>();
    public List<CurrentMeter> CurrentMeters { get; set; } = new();
    public List<Propeller> Propellers { get; set; } = new();
    public List<LocationItem> LocationsViewModel { get; set; } = new List<LocationItem>();
    public List<Area> Areas { get; set; } = new List<Area>();
    public ObservableCollection<WaterAssessment.Models.Assessment> Assessments { get; set; } = new ObservableCollection<WaterAssessment.Models.Assessment>();
    public List<AssessmentItem> AssessmentsViewModel { get; set; } = new List<AssessmentItem>();


    private const string DecimalFormat = "0.00#";

    internal static AssessmentFormPage Instance { get; private set; }

    //***********************************Assessment Dependency Property***************************//

    public static readonly DependencyProperty AssessmentProperty = DependencyProperty.Register(nameof(Assessment),
        typeof(AssessmentItem), typeof(AssessmentFormPage), new PropertyMetadata(null));

    public AssessmentItem Assessment
    {
        get => (AssessmentItem)GetValue(AssessmentProperty);
        set => SetValue(AssessmentProperty, value);
    }

    //*****************************************************************************************//

    public AssessmentFormPage()
    {
        this.InitializeComponent();
        Instance = this;
        DataContext = this;
        Loaded += AssessmentFormPage_Loaded;
    }

    private async void AssessmentFormPage_Loaded(object sender, RoutedEventArgs e)
    {
        CreateRows(Assessment.RowsCount);
        BindTextProperty();
    }

    private void DataGrid_OnLoaded(object sender, RoutedEventArgs e)
    {
        InitAssessmentItemList();
    }

    private void InitAssessmentItemList()
    {
        PersianCalendar pc = new PersianCalendar();
        AssessmentsViewModel?.Clear();
        using var db = new WaterAssessmentContext();

        var assessmentViewModels = db.Assessments
            .Include(a => a.Location)
            .Include(a => a.CurrentMeter)
            .Include(a => a.Propeller)
            .Include(a => a.FormValues)
            .Include(a => a.AssessmentEmployees)
            .ThenInclude(ae => ae.Employee)
            .Select(a => new AssessmentItem()
            {
                AssessmentID = a.AssessmentID,
                Timer = a.Timer,
                Date = $"{pc.GetYear(a.Date)}/{pc.GetMonth(a.Date)}/{pc.GetDayOfMonth(a.Date)}",
                Inserted = a.Inserted,
                Echelon = a.Echelon,
                Openness = a.Openness,
                TotalFlow = a.TotalFlow,
                IsCanal = a.IsCanal,
                LocationName = a.Location.Place,
                CurrentMeterName = a.CurrentMeter.CurrentMeterName,
                PropellerName = a.Propeller.DeviceNumber,
                FormValues = a.FormValues.ToList(),
                EmployeeNames = string.Join(", ", a.AssessmentEmployees
                    .Select(ae => ae.Employee.ToString()).ToList()),
            })
            .ToList();

        AssessmentsViewModel = new(assessmentViewModels);
        dataGrid.ItemsSource = AssessmentsViewModel;
    }

    private void CreateRows(int rows = 4)
    {
        for (int i = 0; i < rows; i++)
        {
            rowNumbersPanel.Children.Add(new Border()
            {
                Width = 70,
                Height = 45,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Child = new TextBlock()
                {
                    Text = $"{i + 1}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
            distancesPanel.Children.Add(new Border()
            {
                Width = 70,
                Height = 45,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Child = new TextBox()
                {
                    Name = $"{i}",
                    VerticalAlignment = VerticalAlignment.Stretch,
                    CornerRadius = new CornerRadius(0),
                    HorizontalTextAlignment = TextAlignment.Center,
                    MaxLength = 4,
                    BorderThickness = new Thickness(0),
                }
            });
            depthsPanel.Children.Add(new Border()
            {
                Width = 70,
                Height = 45,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 2, 1),
                Child = new TextBox()
                {
                    Name = $"{i}",
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalTextAlignment = TextAlignment.Center,
                    MaxLength = 4,
                    BorderThickness = new Thickness(0),
                    CornerRadius = new CornerRadius(0)
                }
            });
            verticalPanel.Children.Add(new Border()
            {
                Width = 88,
                Height = 45,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Child = new TextBox()
                {
                    Name = $"{i}",
                    VerticalAlignment = VerticalAlignment.Stretch,
                    CornerRadius = new CornerRadius(0),
                    HorizontalTextAlignment = TextAlignment.Center,
                    MaxLength = 4,
                    BorderThickness = new Thickness(0),
                    FontSize = 15,
                    IsReadOnly = true
                }
            });
        }

        for (int j = 0; j < (rows * 3); j++)
        {
            radianPanel.Children.Add(new Border()
            {
                Width = 65,
                Height = 15,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Child = new TextBox()
                {
                    Name = $"{j}",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    MaxLength = 4,
                    BorderThickness = new Thickness(0),
                    FontSize = 15
                }
            });
            oneSecPanel.Children.Add(new Border()
            {
                Width = 65,
                Height = 15,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 2, 1),
                Child = new TextBox()
                {
                    Name = $"{j}",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    MaxLength = 4,
                    BorderThickness = new Thickness(0),
                    FontSize = 15,
                    IsReadOnly = true
                }
            });
            pointPanel.Children.Add(new Border()
            {
                Width = 88,
                Height = 15,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Child = new TextBox()
                {
                    Name = $"{j}",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    MaxLength = 4,
                    BorderThickness = new Thickness(0),
                    FontSize = 15,
                    IsReadOnly = true
                }
            });
        }

        for (int i = 0; i < (rows - 1); i++)
        {
            crossPanel.Children.Add(new Border()
            {
                Width = 88,
                Height = 45,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 2, 1),
                Child = new TextBox()
                {
                    Name = $"{i}",
                    VerticalAlignment = VerticalAlignment.Stretch,
                    CornerRadius = new CornerRadius(0),
                    HorizontalTextAlignment = TextAlignment.Center,
                    MaxLength = 4,
                    BorderThickness = new Thickness(0),
                    FontSize = 15,
                    IsReadOnly = true,
                }
            });
            crossDepthPanel.Children.Add(new Border()
            {
                Width = 70,
                Height = 45,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Child = new TextBox()
                {
                    Name = $"{i}",
                    VerticalAlignment = VerticalAlignment.Stretch,
                    CornerRadius = new CornerRadius(0),
                    HorizontalTextAlignment = TextAlignment.Center,
                    MaxLength = 4,
                    BorderThickness = new Thickness(0),
                    FontSize = 15,
                    IsReadOnly = true,
                }
            });
            sectionalWidthPanel.Children.Add(new Border()
            {
                Width = 70,
                Height = 45,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Child = new TextBox()
                {
                    Name = $"{i}",
                    VerticalAlignment = VerticalAlignment.Stretch,
                    CornerRadius = new CornerRadius(0),
                    HorizontalTextAlignment = TextAlignment.Center,
                    MaxLength = 4,
                    BorderThickness = new Thickness(0),
                    FontSize = 15,
                    IsReadOnly = true
                }
            });
            areaPanel.Children.Add(new Border()
            {
                Width = 70,
                Height = 45,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Child = new TextBox()
                {
                    Name = $"{i}",
                    VerticalAlignment = VerticalAlignment.Stretch,
                    CornerRadius = new CornerRadius(0),
                    HorizontalTextAlignment = TextAlignment.Center,
                    MaxLength = 4,
                    BorderThickness = new Thickness(0),
                    FontSize = 15,
                    IsReadOnly = true
                }
            });
            crossFlowPanel.Children.Add(new Border()
            {
                Width = 70,
                Height = 45,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = new TextBox()
                {
                    Name = $"{i}",
                    VerticalAlignment = VerticalAlignment.Stretch,
                    CornerRadius = new CornerRadius(0),
                    HorizontalTextAlignment = TextAlignment.Center,
                    MaxLength = 4,
                    BorderThickness = new Thickness(0),
                    FontSize = 15,
                    IsReadOnly = true
                }
            });
        }
    }

    private List<TextBox> GetPanelTextBoxes(StackPanel panel)
    {
        return panel.Children
            .OfType<Border>()
            .Select(border => border.Child)
            .OfType<TextBox>().ToList();
    }

    private void BindTextProperty()
    {
        var radianPanelTextBoxes = GetPanelTextBoxes(radianPanel);
        var oneSecPanelTextBoxes = GetPanelTextBoxes(oneSecPanel);
        var pointPanelTextBoxes = GetPanelTextBoxes(pointPanel);
        var verticalPanelTextBoxes = GetPanelTextBoxes(verticalPanel);
        var depthsPanelTextBoxes = GetPanelTextBoxes(depthsPanel);
        var crossDepthPanelTextBoxes = GetPanelTextBoxes(crossDepthPanel);
        var sectionalWidthPanelTextBoxes = GetPanelTextBoxes(sectionalWidthPanel);
        var distancesPanelTextBoxes = GetPanelTextBoxes(distancesPanel);
        var areaPanelTextBoxes = GetPanelTextBoxes(areaPanel);
        var crossPanelTextBoxes = GetPanelTextBoxes(crossPanel);
        var crossFlowPanelTextBoxes = GetPanelTextBoxes(crossFlowPanel);

        foreach (var verticalTextBox in verticalPanelTextBoxes)
        {
            verticalTextBox.TextChanged += VerticalTextBox_TextChanged;
        }

        foreach (var depthsTextBox in depthsPanelTextBoxes)
        {
            depthsTextBox.TextChanged += DepthsTextBox_TextChanged;
        }

        foreach (var distanceTextBox in distancesPanelTextBoxes)
        {
            distanceTextBox.TextChanged += DistanceTextBox_TextChanged;
        }

        foreach (var crossDepthTextBox in crossDepthPanelTextBoxes)
        {
            crossDepthTextBox.TextChanged += SectionalWidthTextBox_TextChanged;
        }

        foreach (var sectionalWidthTextBox in sectionalWidthPanelTextBoxes)
        {
            sectionalWidthTextBox.TextChanged += SectionalWidthTextBox_TextChanged;
        }

        foreach (var crossTextBox in crossPanelTextBoxes)
        {
            crossTextBox.TextChanged += CrossTextBox_TextChanged;
        }

        foreach (var areaTextBox in areaPanelTextBoxes)
        {
            areaTextBox.TextChanged += CrossTextBox_TextChanged;
        }

        foreach (var pointTextBox in pointPanelTextBoxes)
        {
            pointTextBox.TextChanged += PointTextBox_TextChanged;
        }

        foreach (var crossFlowTextBox in crossFlowPanelTextBoxes)
        {
            crossFlowTextBox.TextChanged += CrossFlowTextBox_TextChanged;
        }

        for (int i = 0; i < radianPanelTextBoxes.Count; i++)
        {
            Binding bindingFifty = new Binding()
            {
                Source = radianPanelTextBoxes[i],
                Converter = new ToOneSecConverter(),
                ConverterParameter = Assessment.Timer,
                Path = new PropertyPath("Text"),
                //TargetNullValue = null,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            oneSecPanelTextBoxes[i].SetBinding(TextBox.TextProperty, bindingFifty);

            Binding bindingOneSec = new Binding()
            {
                Source = oneSecPanelTextBoxes[i],
                Converter = new RadianToVelocityConverter(),
                ConverterParameter = Assessment.Propeller,
                Path = new PropertyPath("Text"),
                //TargetNullValue = null,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };
            pointPanelTextBoxes[i].SetBinding(TextBox.TextProperty, bindingOneSec);
        }
    }

    private void CrossFlowTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var crossFlowPanelTextBoxes = GetPanelTextBoxes(crossFlowPanel);
        var nonNullTextBoxes = crossFlowPanelTextBoxes.Where(textBox => !string.IsNullOrEmpty(textBox.Text)).ToList();
        if (nonNullTextBoxes.Any())
        {
            var sum = nonNullTextBoxes.Sum(textBox => double.Parse(textBox.Text));
            totalFlowBox.Text = sum.ToString(DecimalFormat);
        }
    }

    private void CrossTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var changedTextBox = sender as TextBox;

        var textBoxesInAreaPanel = GetPanelTextBoxes(areaPanel);
        var textBoxesInCrossPanel = GetPanelTextBoxes(crossPanel);
        var textBoxesInCrossFlowPanel = GetPanelTextBoxes(crossFlowPanel);

        for (int i = 0; i < textBoxesInCrossFlowPanel.Count; i++)
        {
            var crossFlowPanelTextBoxName = textBoxesInCrossFlowPanel[i].Name;
            var areaPanelTextBoxName = textBoxesInAreaPanel[i].Name;
            var crossPanelTextBoxName = textBoxesInCrossPanel[i].Name;

            if (!string.IsNullOrEmpty(changedTextBox.Text))
            {
                if (crossFlowPanelTextBoxName == areaPanelTextBoxName &&
                    crossFlowPanelTextBoxName == crossPanelTextBoxName &&
                    !string.IsNullOrEmpty(textBoxesInAreaPanel[i].Text) &&
                    !string.IsNullOrEmpty(textBoxesInCrossPanel[i].Text))
                {
                    if (double.TryParse(textBoxesInCrossPanel[i].Text, out double cross) &&
                        double.TryParse(textBoxesInAreaPanel[i].Text, out double area))
                    {
                        var crossFlow = cross * area;
                        textBoxesInCrossFlowPanel[i].Text = crossFlow.ToString(DecimalFormat);
                    }
                }
            }
            else
            {
                if (crossFlowPanelTextBoxName == areaPanelTextBoxName &&
                    crossFlowPanelTextBoxName == crossPanelTextBoxName &&
                    (string.IsNullOrEmpty(textBoxesInAreaPanel[i].Text) ||
                     string.IsNullOrEmpty(textBoxesInCrossPanel[i].Text)))
                {
                    textBoxesInCrossFlowPanel[i].Text = null;
                }
            }
        }
    }

    private void SectionalWidthTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var crossDepthPanelTextBoxes = GetPanelTextBoxes(crossDepthPanel);
        var sectionalWidthPanelTextBoxes = GetPanelTextBoxes(sectionalWidthPanel);
        var areaPanelTextBoxes = GetPanelTextBoxes(areaPanel);

        var sectionalWidthTextBox = sender as TextBox;

        for (int i = 0; i < areaPanelTextBoxes.Count; i++)
        {
            var areaPanelTextBoxName = areaPanelTextBoxes[i].Name;
            var crossDepthPanelTextBoxName = crossDepthPanelTextBoxes[i].Name;
            var sectionalWidthPanelTextBoxName = sectionalWidthPanelTextBoxes[i].Name;

            if (!string.IsNullOrEmpty(sectionalWidthTextBox.Text))
            {
                if (areaPanelTextBoxName == crossDepthPanelTextBoxName &&
                    areaPanelTextBoxName == sectionalWidthPanelTextBoxName &&
                    !string.IsNullOrEmpty(crossDepthPanelTextBoxes[i].Text) &&
                    !string.IsNullOrEmpty(sectionalWidthPanelTextBoxes[i].Text))
                {
                    if (double.TryParse(crossDepthPanelTextBoxes[i].Text, out double depth) &&
                        double.TryParse(sectionalWidthPanelTextBoxes[i].Text, out double width))
                    {
                        var area = depth * width;
                        areaPanelTextBoxes[i].Text = area.ToString(DecimalFormat);
                    }
                }
            }
            else
            {
                if (areaPanelTextBoxName == crossDepthPanelTextBoxName &&
                    areaPanelTextBoxName == sectionalWidthPanelTextBoxName &&
                    (string.IsNullOrEmpty(crossDepthPanelTextBoxes[i].Text) ||
                     string.IsNullOrEmpty(sectionalWidthPanelTextBoxes[i].Text)))
                {
                    areaPanelTextBoxes[i].Text = null;
                }
            }
        }
    }

    private void DistanceTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBoxesInDistancesPanel = GetPanelTextBoxes(distancesPanel);
        var textBoxesInSectionalWidthPanel = GetPanelTextBoxes(sectionalWidthPanel);

        var distanceTextBox = sender as TextBox;

        for (int i = 0; i < textBoxesInSectionalWidthPanel.Count; i++)
        {
            var currentIndex = i;
            var nextIndex = i + 1;

            var currentSectionalWidthTextBox = textBoxesInSectionalWidthPanel[currentIndex];

            var currentDistanceText = textBoxesInDistancesPanel[currentIndex].Text;
            var nextDistanceText = textBoxesInDistancesPanel[nextIndex].Text;

            if (!string.IsNullOrEmpty(distanceTextBox.Text))
            {
                if (currentSectionalWidthTextBox.Name == currentIndex.ToString() &&
                    (!string.IsNullOrEmpty(textBoxesInDistancesPanel[currentIndex].Text) &&
                     !string.IsNullOrEmpty(textBoxesInDistancesPanel[nextIndex].Text)))
                {
                    if (double.TryParse(currentDistanceText, out var currentDistance) &&
                        double.TryParse(nextDistanceText, out var nextDistance))
                    {
                        var subtractDistance = nextDistance - currentDistance;
                        currentSectionalWidthTextBox.Text = subtractDistance.ToString(DecimalFormat);
                    }
                }
            }
            else
            {
                if (currentSectionalWidthTextBox.Name == currentIndex.ToString() &&
                    (string.IsNullOrEmpty(currentDistanceText) || string.IsNullOrEmpty(nextDistanceText)))
                {
                    currentSectionalWidthTextBox.Text = null;
                }
            }
        }
    }

    private void DepthsTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBoxesInDepthsPanel = GetPanelTextBoxes(depthsPanel);
        var textBoxesInCrossDepthPanel = GetPanelTextBoxes(crossDepthPanel);

        var depthsTextBox = sender as TextBox;

        for (int i = 0; i < textBoxesInCrossDepthPanel.Count; i++)
        {
            var currentIndex = i;
            var nextIndex = i + 1;

            var currentCrossDepthTextBox = textBoxesInCrossDepthPanel[currentIndex];

            var currentDepthText = textBoxesInDepthsPanel[currentIndex].Text;
            var nextDepthText = textBoxesInDepthsPanel[nextIndex].Text;

            if (!string.IsNullOrEmpty(depthsTextBox.Text))
            {
                if (currentCrossDepthTextBox.Name == currentIndex.ToString() &&
                    !string.IsNullOrEmpty(currentDepthText) && !string.IsNullOrEmpty(nextDepthText))
                {
                    var averageDepth = (double.Parse(nextDepthText) + double.Parse(currentDepthText)) / 2;
                    currentCrossDepthTextBox.Text = averageDepth.ToString("0.00#");
                }
            }
            else
            {
                if (currentCrossDepthTextBox.Name == currentIndex.ToString() &&
                    (string.IsNullOrEmpty(currentDepthText) || string.IsNullOrEmpty(nextDepthText)))
                {
                    currentCrossDepthTextBox.Text = null;
                }
            }
        }
    }

    private void VerticalTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBoxesInVerticalPanel = GetPanelTextBoxes(verticalPanel);
        var textBoxesInCrossPanel = GetPanelTextBoxes(crossPanel);

        var verticalTextBox = sender as TextBox;

        for (int i = 0; i < textBoxesInCrossPanel.Count; i++)
        {
            var currentIndex = i;
            var nextIndex = i + 1;

            var currentCrossTextBox = textBoxesInCrossPanel[currentIndex];

            var currentVerticalText = textBoxesInVerticalPanel[currentIndex].Text;
            var nextVerticalText = textBoxesInVerticalPanel[nextIndex].Text;

            if (!string.IsNullOrEmpty(verticalTextBox.Text))
            {
                if (currentCrossTextBox.Name == currentIndex.ToString() &&
                    !string.IsNullOrEmpty(currentVerticalText) &&
                    !string.IsNullOrEmpty(nextVerticalText))
                {
                    var firstVertical = double.Parse(currentVerticalText);
                    var secondVertical = double.Parse(nextVerticalText);
                    var avgVerticalVelocity = (firstVertical + secondVertical) / 2;
                    currentCrossTextBox.Text = avgVerticalVelocity.ToString("0.00#");
                }
            }
            else
            {
                if (currentCrossTextBox.Name == currentIndex.ToString() &&
                    (string.IsNullOrEmpty(currentVerticalText) ||
                     string.IsNullOrEmpty(nextVerticalText)))
                {
                    currentCrossTextBox.Text = null;
                }
            }
        }
    }

    private void PointTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBoxesInPointPanel = GetPanelTextBoxes(pointPanel);
        var textBoxesInVerticalPanel = GetPanelTextBoxes(verticalPanel);

        var pointTextBox = sender as TextBox;

        for (int j = 0; j < textBoxesInVerticalPanel.Count; j++)
        {
            double sumPointVelocity = 0;
            int divNumber = 0;

            var verticalPanelTextBox = textBoxesInVerticalPanel[j];
            for (int k = 0; k < 3; k++)
            {
                var index = (j * 3) + k;

                var pointPanelTextBox = textBoxesInPointPanel[index];

                if (!string.IsNullOrEmpty(pointTextBox.Text))
                {
                    if (verticalPanelTextBox.Name == j.ToString() &&
                        !string.IsNullOrEmpty(pointPanelTextBox.Text))
                    {
                        var pointValue = double.Parse(pointPanelTextBox.Text);
                        if (pointValue >= 0)
                        {
                            divNumber++;
                            sumPointVelocity += pointValue;
                        }
                    }

                    if (divNumber > 0)
                    {
                        var avgPointVelocity = sumPointVelocity / divNumber;
                        verticalPanelTextBox.Text = avgPointVelocity.ToString("0.###");
                    }
                }
                else
                {
                    if (verticalPanelTextBox.Name == j.ToString() &&
                        !string.IsNullOrEmpty(pointPanelTextBox.Text))
                    {
                        var pointValue = double.Parse(pointPanelTextBox.Text);

                        if (pointValue >= 0)
                        {
                            divNumber++;
                            sumPointVelocity += pointValue;
                        }
                    }

                    if (divNumber == 0)
                    {
                        verticalPanelTextBox.Text = string.Empty;
                    }
                    else
                    {
                        var avgPointVelocity = sumPointVelocity / divNumber;
                        verticalPanelTextBox.Text = avgPointVelocity.ToString("0.###");
                    }
                }
            }
        }
    }

    private void BtnClearFormCells_OnClick(object sender, RoutedEventArgs e)
    {
        
    }

    private async void btnAddAssessment_OnClick(object sender, RoutedEventArgs e)
    {
        List<Assessment_Employee> employees = new List<Assessment_Employee>();
        List<FormValue> formValues = new List<FormValue>();

        Models.Assessment assessment = new Assessment
        {
            Timer = Assessment.Timer,
            Date = Convert.ToDateTime(Assessment.Date),
            Echelon = Assessment.Echelon,
            Openness = Assessment.Openness,
            IsCanal = Assessment.IsCanal,
            PropellerID = Assessment.Propeller.PropellerID,
            CurrentMeterID = Assessment.CurrentMeterID,
            LocationID = Assessment.LocationID,
            TotalFlow = Convert.ToDouble(totalFlowBox.Text),
        };
        await using var db = new WaterAssessmentContext();
        await db.Assessments.AddAsync(assessment);
        await db.SaveChangesAsync();

        if (Assessment.EmployeeID is not null)
        {
            for (int i = 0; i < Assessment.EmployeeID.Length; i++)
            {
                Assessment_Employee employee = new Assessment_Employee
                {
                    AssessmentID = assessment.AssessmentID,
                    EmployeeID = Assessment.EmployeeID[i],
                };

                employees.Add(employee);
            }

            await db.AssessmentEmployees.AddRangeAsync(employees);
        }

        var textBoxesInDistancePanel = GetPanelTextBoxes(distancesPanel);
        var textBoxesInDepthPanel = GetPanelTextBoxes(depthsPanel);
        var radianPanelTextBoxes = GetPanelTextBoxes(radianPanel);

        for (int i = 0; i < textBoxesInDistancePanel.Count; i++)
        {
            string[] radians = new string[3];

            FormValue formValue = new FormValue();
            formValue.AssessmentID = assessment.AssessmentID;
            formValue.Distance = Convert.ToDouble(textBoxesInDistancePanel[i].Text);
            formValue.Depth = Convert.ToDouble(textBoxesInDepthPanel[i].Text);

            for (int k = 0; k < 3; k++)
            {
                var index = (i * 3) + k;
                radians.SetValue(radianPanelTextBoxes[index].Text, k);
            }

            formValue.RadianPerTime_1 = radians[0];
            formValue.RadianPerTime_2 = radians[1];
            formValue.RadianPerTime_3 = radians[2];

            Array.Clear(radians);
            formValues.Add(formValue);
        }

        await db.FormValues.AddRangeAsync(formValues);

        await db.SaveChangesAsync();
        //InitInfoBar.ImplementInfoBar(assessmentInfoBar, InfoBarSeverity.Success,
        //    true, "اطلاعات اندازه گیری مورد نظر شما با موفقیت ثبت شد.");
    }
}