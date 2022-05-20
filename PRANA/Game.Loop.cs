using System.Diagnostics;

namespace PRANA;

public partial class Game
{
    private const int DefaultFrameRate = 60;

    public static int TargetFrameRate
    {
        get => _instance._targetFrameRate;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(
                    "The frame rate must be positive and non-zero.", default(Exception));

            _instance._targetFrameRate = value;

            _instance._targetElapsedTime = TimeSpan.FromTicks((long)(Platform.GetPerformanceFrequency() / value));

            if (_instance._targetElapsedTime > _instance._maxElapsedTime)
                throw new ArgumentOutOfRangeException(
                    "The frame rate resulting target elapsed time cannot exceed MaxElapsedTime", default(Exception));
        }
    }

    public static bool IsFixedTimeStep { get; set; } = true;

    public static TimeSpan MaxElapsedTime
    {
        get => _instance._maxElapsedTime;
        set
        {
            if (value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("The time must be positive", default(Exception));
            }

            if (value < _instance._targetElapsedTime)
            {
                throw new ArgumentOutOfRangeException("The time must be at least equal to TargetElapsedTime",
                    default(Exception));
            }

            _instance._maxElapsedTime = value;
        }
    }

    public static TimeSpan InactiveSleepTime
    {
        get => _instance._inactiveSleepTime;
        set
        {
            if (value < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("The time must be positive.", default(Exception));

            _instance._inactiveSleepTime = value;
        }
    }

    public static bool IsActive { get; private set; } = true;

    private bool _suppressDraw;

    private bool _running;

    private int _targetFrameRate = DefaultFrameRate;

    private TimeSpan _targetElapsedTime = TimeSpan.FromTicks((long)(Platform.GetPerformanceFrequency()/DefaultFrameRate));
    private TimeSpan _inactiveSleepTime = TimeSpan.FromSeconds(0.02);

    private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);

    private TimeSpan _accumElapsedTime;
    private readonly GameTime _gameTime = new ();
    private Stopwatch _gameTimer;
    private long _previousTicks;
    private int _updateFrameLag;


    /// <summary>
    /// Run one iteration of the game loop.
    ///
    /// Makes at least one call to Scene.Update.
    /// and exactly one call to Scene.Draw if drawing is not supressed.
    /// When <see cref="IsFixedTimeStep"/> is set to <code>false</code> this will
    /// make exactly one call to Scene.Update.
    /// </summary>
    private void Tick(Scene scene)
    {

        RetryTick:

            if (!IsActive && InactiveSleepTime.TotalMilliseconds >= 1.0)
            {
                Thread.Sleep((int)InactiveSleepTime.TotalMilliseconds);
            }

            // Advance the accumulated elapsed time.
            if (_gameTimer == null)
            {
                _gameTimer = new Stopwatch();
                _gameTimer.Start();
            }

            var currentTicks = _gameTimer.Elapsed.Ticks;
            _accumElapsedTime += TimeSpan.FromTicks(currentTicks - _previousTicks);
            _previousTicks = currentTicks;

            if (IsFixedTimeStep && _accumElapsedTime < _targetElapsedTime)
            {
                // Sleep for as long as possible without overshooting the update time
                var sleepTime = (_targetElapsedTime - _accumElapsedTime).TotalMilliseconds;

                // We only have a precision timer on Windows, so other platforms may still overshoot

                #if WINDOWS

                TimerHelper.SleepForNoMoreThan(sleepTime);

                #else

                if (sleepTime >= 2.0)
                {
                    Thread.Sleep(1);
                }

                #endif

                goto RetryTick;
            }

            // Do not allow any update to take longer than our maximum.
            if (_accumElapsedTime > _maxElapsedTime)
            {
                _accumElapsedTime = _maxElapsedTime;
            }

            if (IsFixedTimeStep)
            {
                _gameTime.ElapsedGameTime = _targetElapsedTime;
                var stepCount = 0;

                // Perform as many full fixed length time steps as we can.
                while (_accumElapsedTime >= _targetElapsedTime && _running)
                {
                    _gameTime.TotalGameTime += _targetElapsedTime;
                    _accumElapsedTime -= _targetElapsedTime;
                    ++stepCount;

                    DoUpdate(scene, _gameTime);
                }

                //Every update after the first accumulates lag
                _updateFrameLag += Math.Max(0, stepCount - 1);

                //If we think we are running slowly, wait until the lag clears before resetting it
                if (_gameTime.IsRunningSlowly)
                {
                    if (_updateFrameLag == 0)
                    {
                        _gameTime.IsRunningSlowly = false;
                    }
                }
                else if (_updateFrameLag >= 5)
                {
                    //If we lag more than 5 frames, start thinking we are running slowly
                    _gameTime.IsRunningSlowly = true;
                }

                //Every time we just do one update and one draw, then we are not running slowly, so decrease the lag
                if (stepCount == 1 && _updateFrameLag > 0)
                {
                    _updateFrameLag--;
                }

                // Draw needs to know the total elapsed time
                // that occured for the fixed length updates.
                _gameTime.ElapsedGameTime = TimeSpan.FromTicks(_targetElapsedTime.Ticks * stepCount);
                    
            }
            else
            {
                // Perform a single variable length update.
                _gameTime.ElapsedGameTime = _accumElapsedTime;
                _gameTime.TotalGameTime += _accumElapsedTime;
                _accumElapsedTime = TimeSpan.Zero;

                DoUpdate(scene, _gameTime);
            }

            if (_suppressDraw)
            {
                _suppressDraw = false;
            }
            else
            {
                DoDraw(scene, _gameTime);
            }
    }

    private static void DoUpdate(Scene scene, GameTime gameTime)
    {
        scene.Update(gameTime);
    }

    private static void DoDraw(Scene scene, GameTime gameTime)
    {
        scene.Draw(gameTime);
        Graphics.Present();
    }


}