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

        private Color _activeColor;
        private Color _currentDrawColor;
        private readonly Color _highlightColor;
        private readonly Color _normalColor;
        private readonly Vector2 _position;
        private TimeSpan _time;
        private double _timeRemaining;
        private readonly double _highlightDuration = 1000.0;

        public SimpleLabel(Vector2 startpos, string starttext)
        {
            _position = startpos;
            Text = starttext;
            _normalColor = Color.Gray;
            _currentDrawColor = _normalColor;
            _activeColor = Color.White;
            _highlightColor = Color.Red;
            _timeRemaining = -1.0;
        }

        public void Draw(SpriteBatch sb, SpriteFont sf)
        {
            sb.DrawString(sf, Text, _position, _currentDrawColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0);
        }

        public void Update(GameTime gtime, Stopwatch realTimer)
        {
            UpdateHighlightDuration(realTimer);
        }

        public void Activate()
        {
            _currentDrawColor = _activeColor;
        }

        public void Deactivate()
        {
            _currentDrawColor = _normalColor;
        }

        public void HighlightRed(Stopwatch realTimer)
        {
            _time = realTimer.Elapsed;
            _currentDrawColor = _highlightColor;
            _timeRemaining = _highlightDuration;
        }

        private void UpdateHighlightDuration(Stopwatch realTimer)
        {
            if (_currentDrawColor.Equals(_highlightColor))
            {
                _timeRemaining -= realTimer.Elapsed.TotalMilliseconds - _time.TotalMilliseconds;
                _time = realTimer.Elapsed;

                if (_timeRemaining < 0)
                    _currentDrawColor = _normalColor;
            }
        }
    }
}