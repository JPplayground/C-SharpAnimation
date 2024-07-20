using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BlocksAnimation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int Rows = 10; // Number of rows in the grid
        private const int Cols = 10; // Number of columns in the grid
        private const int TotalSquares = Rows * Cols; // Total number of squares

        private int _count = 0; // Counter for tracking filled/cleared squares
        private bool _currentlyAscending = true; // Indicates if squares are being filled
        private bool _currentlyDescending = false; // Indicates if squares are being cleared
        private readonly Stack<int> _randomNumberStack = new Stack<int>(); // Stack for random square indices
        private readonly Dictionary<string, Rectangle> _rectangles = new Dictionary<string, Rectangle>(); // Dictionary for storing rectangles

        private const int BaseSpeed = 100; // Base speed of the timer in milliseconds
        private const int IncreasedSpeed = 25; // Increased speed of the timer in milliseconds
        private readonly DispatcherTimer _timer; // Timer for animation updates

        public MainWindow()
        {
            InitializeComponent();
            InitializeGrid(); // Set up the grid and rectangles
            RefillStack(); // Initialize and shuffle the stack

            // Set up the timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(BaseSpeed)
            };
            _timer.Tick += UpdateAnimation;
            _timer.Start();
        }

        /// <summary>
        /// Initializes the grid with rectangles.
        /// </summary>
        private void InitializeGrid()
        {
            // Create rows and columns
            for (int i = 0; i < Rows; i++)
            {
                MyGrid.RowDefinitions.Add(new RowDefinition());
                MyGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Add rectangles to the grid
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    Rectangle rect = new Rectangle
                    {
                        Fill = Brushes.Blue,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Name = $"Rect_{row}_{col}",
                        Opacity = 0.3 // Initial opacity
                    };

                    // Register name for accessing in code-behind
                    this.RegisterName(rect.Name, rect);

                    // Add rectangle to grid at specific row and column
                    Grid.SetRow(rect, row);
                    Grid.SetColumn(rect, col);
                    MyGrid.Children.Add(rect);

                    // Store the rectangle in the dictionary
                    _rectangles.Add(rect.Name, rect);
                }
            }
        }

        /// <summary>
        /// Fills a specific square by changing its color and animating its opacity.
        /// </summary>
        /// <param name="row">The row index of the square.</param>
        /// <param name="col">The column index of the square.</param>
        private void FillSquare(int row, int col)
        {
            string rectName = $"Rect_{row}_{col}";
            if (_rectangles.TryGetValue(rectName, out Rectangle rect))
            {
                rect.Fill = Brushes.Red;
                AnimateOpacity(rect, 0.3, 1);
            }
        }

        /// <summary>
        /// Clears a specific square by changing its color and animating its opacity.
        /// </summary>
        /// <param name="row">The row index of the square.</param>
        /// <param name="col">The column index of the square.</param>
        private void ClearSquare(int row, int col)
        {
            string rectName = $"Rect_{row}_{col}";
            if (_rectangles.TryGetValue(rectName, out Rectangle rect))
            {
                rect.Fill = Brushes.Blue;
                AnimateOpacity(rect, 1, 0.3);
            }
        }

        /// <summary>
        /// Animates the opacity of a rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to animate.</param>
        /// <param name="from">The starting opacity.</param>
        /// <param name="to">The ending opacity.</param>
        private void AnimateOpacity(Rectangle rect, double from, double to)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(0.5) // Duration of the animation
            };
            rect.BeginAnimation(OpacityProperty, animation);
        }

        /// <summary>
        /// Handles the timer tick event to update the animation.
        /// </summary>
        private void UpdateAnimation(object sender, EventArgs e)
        {
            // Get random row and column
            var (row, col) = GetRandomRowAndCol();

            // Squares are left to fill
            if (_currentlyAscending && (_count != TotalSquares - 1))
            {
                FillSquare(row, col); // Fill in random square
                _count++; // Increment count
            }
            // One square left to fill
            else if (_currentlyAscending && (_count == TotalSquares - 1))
            {
                FillSquare(row, col); // Fill last square
                ReverseDirection(); // Reverse direction
                SpeedUpTimer(); // Speed up timer       
                RefillStack(); // Refill stack
            }
            // Squares left to remove
            else if (_currentlyDescending && _count != 0)
            {
                ClearSquare(row, col); // Remove squares
                _count--; // Decrement count
            }
            // One square left to remove
            else if (_currentlyDescending && _count == 0)
            {
                ClearSquare(row, col); // Remove last square
                ReverseDirection(); // Reverse direction
                NormalizeTimerSpeed(); // Normalize timer speed
                RefillStack(); // Refill stack
            }
        }

        /// <summary>
        /// Reverses the direction of filling and clearing squares.
        /// </summary>
        private void ReverseDirection()
        {
            _currentlyAscending = !_currentlyAscending;
            _currentlyDescending = !_currentlyDescending;
        }

        /// <summary>
        /// Speeds up the timer interval.
        /// </summary>
        private void SpeedUpTimer()
        {
            _timer.Interval = TimeSpan.FromMilliseconds(IncreasedSpeed);
        }

        /// <summary>
        /// Normalizes the timer interval to the base speed.
        /// </summary>
        private void NormalizeTimerSpeed()
        {
            _timer.Interval = TimeSpan.FromMilliseconds(BaseSpeed);
        }

        /// <summary>
        /// Refills the stack with shuffled indices.
        /// </summary>
        private void RefillStack()
        {
            // Generate and shuffle array of ints [0 - 99]
            int[] nums = Enumerable.Range(0, TotalSquares).ToArray();
            new Random().Shuffle(nums);

            // Clear the stack before refilling
            _randomNumberStack.Clear();

            // Add shuffled numbers to stack
            foreach (var num in nums)
            {
                _randomNumberStack.Push(num);
            }
        }

        /// <summary>
        /// Retrieves a random row and column from the stack.
        /// </summary>
        /// <returns>A tuple containing the row and column indices.</returns>
        private (int, int) GetRandomRowAndCol()
        {
            if (_randomNumberStack.Count == 0)
            {
                Console.WriteLine("Stack is empty and we are trying to get from it. Something is wrong");
                Environment.Exit(1);
            }

            int randomNumber = _randomNumberStack.Pop();
            int row = randomNumber / Cols;
            int col = randomNumber % Cols;

            return (row, col);
        }
    }
}
