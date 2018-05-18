using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace InputHandlers.Sample
{
    public class SimpleLabel
    {
        public string Text { get; set; }

        private Color _activeColour;
        private Color _currentDrawColour;
        private readonly Color _highlightColour;
        private readonly Color _normalColour;
        private readonly Vector2 _position;
        private TimeSpan _time;
        private double _timeRemaining;
        private readonly double _highlightDuration = 1000.0;

        public SimpleLabel(Vector2 startpos, string starttext)
        {
            Text = starttext;
            _position = startpos;
            _normalColour = Color.Gray;
            _currentDrawColour = _normalColour;
            _activeColour = Color.White;
            _highlightColour = Color.Red;
            _timeRemaining = -1.0;
        }

        public void Draw(SpriteBatch sb, SpriteFont sf)
        {
            sb.DrawString(sf, Text, _position, _currentDrawColour, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0);
        }

        public void Update(GameTime gtime, Stopwatch realTimer)
        {
            UpdateHighlightDuration(realTimer);
        }

        public void Activate()
        {
            _currentDrawColour = _activeColour;
        }

        public void Deactivate()
        {
            _currentDrawColour = _normalColour;
        }

        public void HighlightRed(Stopwatch realTimer)
        {
            _time = realTimer.Elapsed;
            _currentDrawColour = _highlightColour;
            _timeRemaining = _highlightDuration;
        }

        private void UpdateHighlightDuration(Stopwatch realTimer)
        {
            if (_currentDrawColour.Equals(_highlightColour))
            {
                _timeRemaining -= realTimer.Elapsed.TotalMilliseconds - _time.TotalMilliseconds;
                _time = realTimer.Elapsed;

                if (_timeRemaining < 0)
                    _currentDrawColour = _normalColour;
            }
        }
    }
}