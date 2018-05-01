using System;

using InputHandlers.Mouse;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Input;

using NSubstitute;

namespace InputHandlers.Tests
{
    [TestClass]
    public class MouseInputTests
    {
        private MouseInput _mouseInput;
        private IMouseHandler _mouseHandler;

        [TestInitialize]
        public void Setup()
        {
            _mouseHandler = Substitute.For<IMouseHandler>();
            _mouseInput = new MouseInput();

            _mouseInput.Subscribe(_mouseHandler);
        }

        [TestMethod]
        public void MouseInput_Should_Call_No_Handlers_When_Polling_With_Blank_MouseState()
        {
            // Arrange
            var mouseState = new MouseState();

            // Act
            _mouseInput.Poll(mouseState);

            // Assert
            _mouseHandler.DidNotReceive().HandleMouseScrollWheelMove(Arg.Any<MouseState>(), Arg.Any<int>());
            _mouseHandler.DidNotReceive().HandleLeftMouseClick(Arg.Any<MouseState>());
            _mouseHandler.DidNotReceive().HandleLeftMouseDoubleClick(Arg.Any<MouseState>());
            _mouseHandler.DidNotReceive().HandleLeftMouseDown(Arg.Any<MouseState>());
            _mouseHandler.DidNotReceive().HandleLeftMouseDragDone(Arg.Any<MouseState>(), Arg.Any<MouseState>());
            _mouseHandler.DidNotReceive().HandleLeftMouseDragging(Arg.Any<MouseState>(), Arg.Any<MouseState>());
            _mouseHandler.DidNotReceive().HandleLeftMouseUp(Arg.Any<MouseState>());
            _mouseHandler.DidNotReceive().HandleMouseMoving(Arg.Any<MouseState>());
            _mouseHandler.DidNotReceive().HandleRightMouseClick(Arg.Any<MouseState>());
            _mouseHandler.DidNotReceive().HandleRightMouseDoubleClick(Arg.Any<MouseState>());
            _mouseHandler.DidNotReceive().HandleRightMouseDown(Arg.Any<MouseState>());
            _mouseHandler.DidNotReceive().HandleRightMouseDragDone(Arg.Any<MouseState>(), Arg.Any<MouseState>());
            _mouseHandler.DidNotReceive().HandleRightMouseDragging(Arg.Any<MouseState>(), Arg.Any<MouseState>());
            _mouseHandler.DidNotReceive().HandleRightMouseUp(Arg.Any<MouseState>());
        }
    }
}
