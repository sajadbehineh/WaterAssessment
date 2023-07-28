using Windows.Globalization;
using WaterAssessment.Converters;

namespace WaterAssessment.Views;

public sealed partial class AssessmentFormPage : Page
{
    private const string DecimalFormat = "0.00#";
    private int time;

    public AssessmentFormPage()
    {
        this.InitializeComponent();
        CreateRows();
        BindTextProperty();
    }

    Propeller propeller4 = new()
    {
        AValue = 0.1391,
        BValue = 0.0351,
        DeviceNumber = "123456"
    };

    Propeller propeller3 = new()
    {
        AValue = 0.1323,
        BValue = 0.0407,
        DeviceNumber = "123456"
    };

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
            fiftyPanel.Children.Add(new Border()
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
        //var fiftyPanelTextBoxes = GetPanelTextBoxes(fiftyPanel);
        //var oneSecPanelTextBoxes = GetPanelTextBoxes(oneSecPanel);
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

        //for (int i = 0; i < fiftyPanelTextBoxes.Count; i++)
        //{
        //    Binding bindingFifty = new Binding()
        //    {
        //        Source = fiftyPanelTextBoxes[i],
        //        Converter = new ToOneSecConverter(),
        //        ConverterParameter = t,
        //        Path = new PropertyPath("Text"),
        //        //TargetNullValue = null,
        //        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        //    };
        //    oneSecPanelTextBoxes[i].SetBinding(TextBox.TextProperty, bindingFifty);

        //    Binding bindingOneSec = new Binding()
        //    {
        //        Source = oneSecPanelTextBoxes[i],
        //        Converter = new RadianToVelocityConverter(),
        //        ConverterParameter = propeller,
        //        Path = new PropertyPath("Text"),
        //        //TargetNullValue = null,
        //        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        //    };
        //    pointPanelTextBoxes[i].SetBinding(TextBox.TextProperty, bindingOneSec);
        //}
    }

    private void CrossFlowTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var crossFlowPanelTextBoxes = GetPanelTextBoxes(crossFlowPanel);
        var nonNullTextBoxes = crossFlowPanelTextBoxes.Where(textBox => !string.IsNullOrEmpty(textBox.Text)).ToList();
        if (nonNullTextBoxes.Any())
        {
            var sum = nonNullTextBoxes.Sum(textBox => double.Parse(textBox.Text));
            //var average = sum / nonNullTextBoxes.Count();
            flowAvg.Text = sum.ToString(DecimalFormat);
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

    private void DatePicker_OnLoaded(object sender, RoutedEventArgs e)
    {
        datePicker.CalendarIdentifier = CalendarIdentifiers.Persian;
        datePicker.SelectedDate = System.DateTime.Today;
    }

    private void CmbTimer_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var fiftyPanelTextBoxes = GetPanelTextBoxes(fiftyPanel);
        var oneSecPanelTextBoxes = GetPanelTextBoxes(oneSecPanel);
        var pointPanelTextBoxes = GetPanelTextBoxes(pointPanel);
        var cmb = sender as ComboBox;
        time = (int)cmb.SelectedValue;
        for (int i = 0; i < fiftyPanelTextBoxes.Count; i++)
        {
            Binding bindingFifty = new Binding()
            {
                Source = fiftyPanelTextBoxes[i],
                Converter = new ToOneSecConverter(),
                ConverterParameter = time,
                Path = new PropertyPath("Text"),
                //TargetNullValue = null,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            oneSecPanelTextBoxes[i].SetBinding(TextBox.TextProperty, bindingFifty);

            Binding bindingOneSec = new Binding()
            {
                Source = oneSecPanelTextBoxes[i],
                Converter = new RadianToVelocityConverter(),
                ConverterParameter = propeller4,
                Path = new PropertyPath("Text"),
                //TargetNullValue = null,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };
            pointPanelTextBoxes[i].SetBinding(TextBox.TextProperty, bindingOneSec);
        }
    }
}