﻿using System;
using System.Linq;

using InputHandlers.Mouse;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Xna.Framework.Input;

using NSubstitute;
using NSubstitute.ClearExtensions;

namespace InputHandlers.Tests;

[TestClass]
public class MouseInputTests
{
    private MouseInput _mouseInput;
    private IMouseHandler _mouseHandler;
    private TestStopwatchProvider _testStopwatchProvider;

    [TestInitialize]
    public void Setup()
    {
        _testStopwatchProvider = new TestStopwatchProvider();
        _mouseHandler = Substitute.For<IMouseHandler>();
        _mouseInput = new MouseInput(_testStopwatchProvider);

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
        _mouseHandler.DidNotReceive().HandleLeftMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseDown(Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseDragDone(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseDragging(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMouseMoving(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseDown(Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseDragDone(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseDragging(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Increment_UpdateNumber_On_Poll()
    {
        // Arrange
        var secondMouseHandler = Substitute.For<IMouseHandler>();

        _mouseInput.Subscribe(secondMouseHandler);

        var mouseState = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        // Act
        _mouseInput.Poll(mouseState);

        // Assert
        Assert.AreEqual(1, _mouseInput.UpdateNumber);

        // Act - poll 2
        _mouseInput.Poll(mouseState);

        // Assert = poll 2
        Assert.AreEqual(2, _mouseInput.UpdateNumber);
    }

    [TestMethod]
    public void MouseInput_Should_Reset_To_Stationary_State_And_Set_UpdateNumber_To_Zero_When_Resetting()
    {
        // Arrange
        var mouseState = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseState);

        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.Reset();

        // Assert
        Assert.AreEqual(0, _mouseInput.UpdateNumber);

        // Act - poll again with mouse released, state is stationary and so no mouse up will occur
        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStateReleased);

        _mouseHandler.DidNotReceive().HandleLeftMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Broadcast_To_Both_Handlers_When_MouseInput_Has_Two_Subscriptions()
    {
        // Arrange
        var secondMouseHandler = Substitute.For<IMouseHandler>();

        _mouseInput.Subscribe(secondMouseHandler);

        var mouseState = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        // Act
        _mouseInput.Poll(mouseState);

        // Assert
        _mouseHandler.Received().HandleLeftMouseDown(Arg.Is(mouseState));
        secondMouseHandler.Received().HandleLeftMouseDown(Arg.Is(mouseState));
    }

    [TestMethod]
    public void MouseInput_Should_Subscribe_And_Unsubscribe_Correctly()
    {
        // Arrange
        var secondMouseHandler = Substitute.For<IMouseHandler>();

        _mouseInput.Subscribe(secondMouseHandler);
        _mouseInput.Unsubscribe(_mouseHandler);
            
        var mouseState = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        // Act
        _mouseInput.Poll(mouseState);

        // Assert
        _mouseHandler.DidNotReceive().HandleLeftMouseDown(Arg.Any<MouseState>());
        secondMouseHandler.Received().HandleLeftMouseDown(Arg.Is(mouseState));
    }
    
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public void Remove_Subscription_When_Same_Handler_Is_Also_In_Pending_Add_And_Remove_Timestamp_Is_Greater_Or_Equal_Then_Subscription_Is_Removed(int advanceTime)
    {
        // Arrange
        var secondMouseHandler = Substitute.For<IMouseHandler>();

        _mouseInput.Unsubscribe(_mouseHandler);
        _mouseInput.Subscribe(secondMouseHandler);
        _testStopwatchProvider.AdvanceByMilliseconds(advanceTime);
        _mouseInput.Unsubscribe(secondMouseHandler);
            
        var mouseState = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        // Act
        _mouseInput.Poll(mouseState);

        // Assert
        _mouseHandler.DidNotReceive().HandleLeftMouseDown(Arg.Any<MouseState>());
        secondMouseHandler.DidNotReceive().HandleLeftMouseDown(Arg.Any<MouseState>());
    }
    
    [TestMethod]
    public void Remove_Subscription_When_Same_Handler_Is_Also_In_Pending_Add_And_Remove_Timestamp_Is_Less_Then_Subscription_Is_Added()
    {
        // Arrange
        var secondMouseHandler = Substitute.For<IMouseHandler>();

        _mouseInput.Unsubscribe(_mouseHandler);
        _mouseInput.Subscribe(secondMouseHandler);
        _mouseInput.Unsubscribe(secondMouseHandler);
        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Subscribe(secondMouseHandler);
            
        var mouseState = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        // Act
        _mouseInput.Poll(mouseState);

        // Assert
        _mouseHandler.DidNotReceive().HandleLeftMouseDown(Arg.Any<MouseState>());
        secondMouseHandler.Received().HandleLeftMouseDown(Arg.Is(mouseState));
    }  

    [TestMethod]
    public void MouseInput_Can_Subscribe_While_Within_Handler_Of_Another_Subscription()
    {
        // Arrange
        var secondMouseHandler = Substitute.For<IMouseHandler>();

        var mouseStateDown = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseHandler
            .When(k => k.HandleLeftMouseDown(Arg.Is(mouseStateDown)))
            .Do(ci => _mouseInput.Subscribe(secondMouseHandler));

        // Act
        _mouseInput.Poll(mouseStateDown);

        // Assert
        _mouseHandler.Received().HandleLeftMouseDown(Arg.Is(mouseStateDown));
        secondMouseHandler.DidNotReceive().HandleLeftMouseDown(Arg.Any<MouseState>());

        // Arrange - second handler should now be subscribed
        _mouseHandler.ClearSubstitute();

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        // Act
        _mouseInput.Poll(mouseStateReleased);

        // Assert
        _mouseHandler.Received().HandleLeftMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStateDown));
        secondMouseHandler.Received().HandleLeftMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStateDown));
    }

    [TestMethod]
    public void MouseInput_Can_Unsubscribe_While_Within_Handler_Of_Another_Subscription()
    {
        // Arrange
        var secondMouseHandler = Substitute.For<IMouseHandler>();
        _mouseInput.Subscribe(secondMouseHandler);

        var mouseStateDown = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseHandler
            .When(k => k.HandleLeftMouseDown(Arg.Is(mouseStateDown)))
            .Do(ci => _mouseInput.Unsubscribe(secondMouseHandler));

        // Act
        _mouseInput.Poll(mouseStateDown);

        // Assert
        _mouseHandler.Received().HandleLeftMouseDown(Arg.Is(mouseStateDown));
        secondMouseHandler.Received().HandleLeftMouseDown(Arg.Is(mouseStateDown));

        // Arrange - second handler should now be subscribed
        _mouseHandler.ClearSubstitute();
        secondMouseHandler.ClearReceivedCalls();

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        // Act
        _mouseInput.Poll(mouseStateReleased);

        // Assert
        _mouseHandler.Received().HandleLeftMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStateDown));
        secondMouseHandler.DidNotReceive().HandleLeftMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Ignore_Second_Subscription_Of_Same_Handler()
    {
        // Arrange
        _mouseInput.Subscribe(_mouseHandler);

        var mouseState = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        // Act
        _mouseInput.Poll(mouseState);

        // Assert
        Assert.AreEqual(1, _mouseHandler.ReceivedCalls().Count());

        _mouseHandler.Received().HandleLeftMouseDown(Arg.Is(mouseState));
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleMouseMoving_When_Mouse_Changes_Position()
    {
        // Arrange
        var mouseStateOrigin = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStateOrigin);

        var mouseStateMoving = new MouseState(
            1,
            1,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);

        // Act
        _mouseInput.Poll(mouseStateMoving);
            
        // Assert
        _mouseHandler.Received().HandleMouseMoving(Arg.Is(mouseStateMoving), Arg.Is(mouseStateOrigin));
    }

    [DataTestMethod]
    [DataRow(-1)]
    [DataRow(1)]
    [DataRow(-2)]
    [DataRow(2)]
    public void MouseInput_Should_Call_HandleMouseScrollWheelMove_When_Scrollwheel_Moves(int scrollWheelMoveAmount)
    {
        // Arrange
        var mouseStateOrigin = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStateOrigin);

        var mouseStateMouseWheelMove = new MouseState(
            0,
            0,
            scrollWheelMoveAmount,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);

        // Act
        _mouseInput.Poll(mouseStateMouseWheelMove);

        // Assert
        _mouseHandler.Received().HandleMouseScrollWheelMove(Arg.Is(mouseStateMouseWheelMove), scrollWheelMoveAmount);
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleLeftMouseDown_When_Left_Is_Pressed()
    {
        // Arrange
        var mouseState = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        // Act
        _mouseInput.Poll(mouseState);

        // Assert
        _mouseHandler.Received().HandleLeftMouseDown(Arg.Is(mouseState));
    }

    [TestMethod]
    public void MouseInput_Should_Do_Nothing_When_Left_Is_Pressed_When_Left_Button_Disabled()
    {
        // Arrange
        var mouseState = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.IsLeftButtonEnabled = false;

        // Act
        _mouseInput.Poll(mouseState);

        // Assert
        _mouseHandler.DidNotReceive().HandleLeftMouseDown(Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleLeftMouseClick_And_Up_When_Left_Released()
    {
        // Arrange
        var mouseStatePressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressed);

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.Poll(mouseStateReleased);

        // Assert
        _mouseHandler.Received().HandleLeftMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressed));
        _mouseHandler.Received().HandleLeftMouseClick(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressed));
        _mouseHandler.DidNotReceive().HandleLeftMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleLeftMouseClick_And_Up_While_Within_Tolerance()
    {
        // Arrange
        var mouseStatePressedOrigin = new MouseState(
            _mouseInput.DragVariance,
            _mouseInput.DragVariance,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressedOrigin);
        _mouseHandler.ClearReceivedCalls();

        var mouseStateWithinDragDropToleranceMax = new MouseState(
            _mouseInput.DragVariance + _mouseInput.DragVariance,
            _mouseInput.DragVariance + _mouseInput.DragVariance,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateWithinDragDropToleranceMax);

        var mouseStateWithinDragDropToleranceMin = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateWithinDragDropToleranceMin);

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);

        // Act
        _mouseInput.Poll(mouseStateReleased);

        // Assert
        _mouseHandler.Received().HandleLeftMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.Received().HandleLeftMouseClick(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.DidNotReceive().HandleLeftMouseDragDone(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseDragging(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMouseMoving(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleLeftMouseDoubleClick_Immediately_On_Second_Click_And_Suppress_MouseUp()
    {
        // Arrange
        var mouseStatePressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressed);

        var mouseStateFirstRelease = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateFirstRelease);

        var mouseStateSecondClick = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.Elapsed = new TimeSpan(0, 0, 0, 0, _mouseInput.DoubleClickDetectionTimeDelay);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.Poll(mouseStateSecondClick);

        // Assert
        _mouseHandler.Received().HandleLeftMouseDoubleClick(Arg.Is(mouseStateSecondClick), Arg.Is(mouseStatePressed));
        _mouseHandler.DidNotReceive().HandleLeftMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseDown(Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());

        // Act - Releasing
        _mouseHandler.ClearReceivedCalls();

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateReleased);

        // Assert - Releasing
        _mouseHandler.DidNotReceive().HandleLeftMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseDown(Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }
    
    [TestMethod]
    public void MouseInput_Should_Not_Call_HandleLeftMouseDoubleClick_If_ResetDoubleClickDetection_Is_Called()
    {
        // Arrange
        var mouseStatePressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressed);

        var mouseStateFirstRelease = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateFirstRelease);

        var mouseStateSecondClick = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.Elapsed = new TimeSpan(0, 0, 0, 0, _mouseInput.DoubleClickDetectionTimeDelay);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.ResetDoubleClickDetection();
        _mouseInput.Poll(mouseStateSecondClick);

        // Assert
        _mouseHandler.Received().HandleLeftMouseDown(Arg.Is(mouseStateSecondClick));
        _mouseHandler.DidNotReceive().HandleLeftMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());

        // Act - Releasing
        _mouseHandler.ClearReceivedCalls();

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateReleased);

        // Assert - Releasing
        _mouseHandler.DidNotReceive().HandleLeftMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseDown(Arg.Any<MouseState>());
        _mouseHandler.Received().HandleLeftMouseClick(Arg.Is(mouseStateReleased), Arg.Is(mouseStateSecondClick));
        _mouseHandler.Received().HandleLeftMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStateSecondClick));
    }

    [TestMethod]
    public void MouseInput_Should_Not_Call_HandleLeftMouseDoubleClick_If_DoubleClickDetection_Time_Has_Passed()
    {
        // Arrange
        var mouseStatePressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressed);

        var mouseStateFirstRelease = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateFirstRelease);

        var mouseStateSecondClick = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.Elapsed = new TimeSpan(0, 0, 0, 0, _mouseInput.DoubleClickDetectionTimeDelay + 1);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.Poll(mouseStateSecondClick);

        // Assert
        _mouseHandler.Received().HandleLeftMouseDown(Arg.Is(mouseStateSecondClick));

        _mouseHandler.DidNotReceive().HandleLeftMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());

        // Act - Releasing
        _mouseHandler.ClearReceivedCalls();

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateReleased);

        // Assert - Releasing
        _mouseHandler.Received().HandleLeftMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStateSecondClick));
        _mouseHandler.Received().HandleLeftMouseClick(Arg.Is(mouseStateReleased), Arg.Is(mouseStateSecondClick));
        _mouseHandler.DidNotReceive().HandleLeftMouseDragDone(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseDragging(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMouseMoving(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [DataTestMethod]
    [DataRow(-1)]
    [DataRow(1)]
    public void MouseInput_Should_Call_HandleLeftMouseDragging_When_Dragging_With_Left_Down(int varianceMultiplier)
    {
        // Arrange
        var startingPosition = _mouseInput.DragVariance * 2;

        var mouseStatePressedOrigin = new MouseState(
            startingPosition,
            startingPosition,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressedOrigin);
        _mouseHandler.ClearReceivedCalls();

        var mouseStateDragging = new MouseState(
            startingPosition + (_mouseInput.DragVariance + 1) * varianceMultiplier,
            startingPosition + (_mouseInput.DragVariance + 1) * varianceMultiplier,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);

        // Act
        _mouseInput.Poll(mouseStateDragging);

        // Assert
        _mouseHandler.Received().HandleLeftMouseDragging(Arg.Is(mouseStateDragging), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.DidNotReceive().HandleMouseMoving(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleLeftMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleLeftMouseDragDone_And_Left_Up_When_Releasing_Left_After_Drag()
    {
        // Arrange
        var mouseStatePressedOrigin = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressedOrigin);

        var mouseStateDragging = new MouseState(
            _mouseInput.DragVariance + 1,
            _mouseInput.DragVariance + 1,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);

        _mouseInput.Poll(mouseStateDragging);

        var mouseStateReleased = new MouseState(
            _mouseInput.DragVariance + 1,
            _mouseInput.DragVariance + 1,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.Poll(mouseStateReleased);

        // Assert
        _mouseHandler.Received().HandleLeftMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.Received().HandleLeftMouseDragDone(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.DidNotReceive().HandleLeftMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleRightMouseDown_When_Right_Is_Pressed()
    {
        // Arrange
        var mouseState = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released
        );

        // Act
        _mouseInput.Poll(mouseState);

        // Assert
        _mouseHandler.Received().HandleRightMouseDown(Arg.Is(mouseState));
    }

    [TestMethod]
    public void MouseInput_Should_Do_Nothing_When_Right_Is_Pressed_When_Right_Button_Disabled()
    {
        // Arrange
        var mouseState = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.IsRightButtonEnabled = false;

        // Act
        _mouseInput.Poll(mouseState);

        // Assert
        _mouseHandler.DidNotReceive().HandleRightMouseDown(Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleRightMouseClick_And_Up_When_Right_Released()
    {
        // Arrange
        var mouseStatePressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressed);

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.Poll(mouseStateReleased);

        // Assert
        _mouseHandler.Received().HandleRightMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressed));
        _mouseHandler.Received().HandleRightMouseClick(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressed));
        _mouseHandler.DidNotReceive().HandleRightMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleRightMouseClick_And_Up_While_Within_Tolerance()
    {
        // Arrange
        var mouseStatePressedOrigin = new MouseState(
            _mouseInput.DragVariance,
            _mouseInput.DragVariance,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released);

        _mouseInput.Poll(mouseStatePressedOrigin);
        _mouseHandler.ClearReceivedCalls();

        var mouseStateWithinDragDropToleranceMax = new MouseState(
            _mouseInput.DragVariance + _mouseInput.DragVariance,
            _mouseInput.DragVariance + _mouseInput.DragVariance,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released);

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateWithinDragDropToleranceMax);

        var mouseStateWithinDragDropToleranceMin = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateWithinDragDropToleranceMin);

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);

        // Act
        _mouseInput.Poll(mouseStateReleased);

        // Assert
        _mouseHandler.Received().HandleRightMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.Received().HandleRightMouseClick(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.DidNotReceive().HandleRightMouseDragDone(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseDragging(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMouseMoving(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleRightMouseDoubleClick_Immediately_On_Second_Click_And_Suppress_MouseUp()
    {
        // Arrange
        var mouseStatePressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released);

        _mouseInput.Poll(mouseStatePressed);

        var mouseStateFirstRelease = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateFirstRelease);

        var mouseStateSecondClick = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released);

        _testStopwatchProvider.Elapsed = new TimeSpan(0, 0, 0, 0, _mouseInput.DoubleClickDetectionTimeDelay);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.Poll(mouseStateSecondClick);

        // Assert
        _mouseHandler.Received().HandleRightMouseDoubleClick(Arg.Is(mouseStateSecondClick), Arg.Is(mouseStatePressed));
        _mouseHandler.DidNotReceive().HandleRightMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseDown(Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());

        // Act - Releasing
        _mouseHandler.ClearReceivedCalls();

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateReleased);

        // Assert - Releasing
        _mouseHandler.DidNotReceive().HandleRightMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseDown(Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }
    
    [TestMethod]
    public void MouseInput_Should_Not_Call_HandleRightMouseDoubleClick_If_ResetDoubleClickDetection_Is_Called()
    {
        // Arrange
        var mouseStatePressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released);

        _mouseInput.Poll(mouseStatePressed);

        var mouseStateFirstRelease = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateFirstRelease);

        var mouseStateSecondClick = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released);

        _testStopwatchProvider.Elapsed = new TimeSpan(0, 0, 0, 0, _mouseInput.DoubleClickDetectionTimeDelay);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.ResetDoubleClickDetection();
        _mouseInput.Poll(mouseStateSecondClick);

        // Assert
        _mouseHandler.Received().HandleRightMouseDown(Arg.Is(mouseStateSecondClick));
        _mouseHandler.DidNotReceive().HandleRightMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());

        // Act - Releasing
        _mouseHandler.ClearReceivedCalls();

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateReleased);

        // Assert - Releasing
        _mouseHandler.DidNotReceive().HandleRightMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseDown(Arg.Any<MouseState>());
        _mouseHandler.Received().HandleRightMouseClick(Arg.Is(mouseStateReleased), Arg.Is(mouseStateSecondClick));
        _mouseHandler.Received().HandleRightMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStateSecondClick));
    }

    [TestMethod]
    public void MouseInput_Should_Not_Call_HandleRightMouseDoubleClick_If_DoubleClickDetection_Time_Has_Passed()
    {
        // Arrange
        var mouseStatePressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released);

        _mouseInput.Poll(mouseStatePressed);

        var mouseStateFirstRelease = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateFirstRelease);

        var mouseStateSecondClick = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released);

        _testStopwatchProvider.Elapsed = new TimeSpan(0, 0, 0, 0, _mouseInput.DoubleClickDetectionTimeDelay + 1);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.Poll(mouseStateSecondClick);

        // Assert
        _mouseHandler.Received().HandleRightMouseDown(Arg.Is(mouseStateSecondClick));

        _mouseHandler.DidNotReceive().HandleRightMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());

        // Act - Releasing
        _mouseHandler.ClearReceivedCalls();

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateReleased);

        // Assert - Releasing
        _mouseHandler.Received().HandleRightMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStateSecondClick));
        _mouseHandler.Received().HandleRightMouseClick(Arg.Is(mouseStateReleased), Arg.Is(mouseStateSecondClick));
        _mouseHandler.DidNotReceive().HandleRightMouseDragDone(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseDragging(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMouseMoving(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [DataTestMethod]
    [DataRow(-1)]
    [DataRow(1)]
    public void MouseInput_Should_Call_HandleRightMouseDragging_When_Dragging_With_Right_Down(int varianceMultiplier)
    {
        // Arrange
        var startingPosition = _mouseInput.DragVariance * 2;

        var mouseStatePressedOrigin = new MouseState(
            startingPosition,
            startingPosition,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released);

        _mouseInput.Poll(mouseStatePressedOrigin);
        _mouseHandler.ClearReceivedCalls();

        var mouseStateDragging = new MouseState(
            startingPosition + (_mouseInput.DragVariance + 1) * varianceMultiplier,
            startingPosition + (_mouseInput.DragVariance + 1) * varianceMultiplier,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);

        // Act
        _mouseInput.Poll(mouseStateDragging);

        // Assert
        _mouseHandler.Received().HandleRightMouseDragging(Arg.Is(mouseStateDragging), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.DidNotReceive().HandleMouseMoving(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleRightMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleRightMouseDragDone_And_Right_Up_When_Releasing_Right_After_Drag()
    {
        // Arrange
        var mouseStatePressedOrigin = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressedOrigin);

        var mouseStateDragging = new MouseState(
            _mouseInput.DragVariance + 1,
            _mouseInput.DragVariance + 1,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);

        _mouseInput.Poll(mouseStateDragging);

        var mouseStateReleased = new MouseState(
            _mouseInput.DragVariance + 1,
            _mouseInput.DragVariance + 1,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.Poll(mouseStateReleased);

        // Assert
        _mouseHandler.Received().HandleRightMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.Received().HandleRightMouseDragDone(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.DidNotReceive().HandleRightMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleMiddleMouseDown_When_Middle_Is_Pressed()
    {
        // Arrange
        var mouseState = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        // Act
        _mouseInput.Poll(mouseState);

        // Assert
        _mouseHandler.Received().HandleMiddleMouseDown(Arg.Is(mouseState));
    }

    [TestMethod]
    public void MouseInput_Should_Do_Nothing_When_Middle_Is_Pressed_When_Middle_Button_Disabled()
    {
        // Arrange
        var mouseState = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.IsMiddleButtonEnabled = false;

        // Act
        _mouseInput.Poll(mouseState);

        // Assert
        _mouseHandler.DidNotReceive().HandleMiddleMouseDown(Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleMiddleMouseClick_And_Up_When_Middle_Released()
    {
        // Arrange
        var mouseStatePressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressed);

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.Poll(mouseStateReleased);

        // Assert
        _mouseHandler.Received().HandleMiddleMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressed));
        _mouseHandler.Received().HandleMiddleMouseClick(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressed));
        _mouseHandler.DidNotReceive().HandleMiddleMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleMiddleMouseClick_And_Up_While_Within_Tolerance()
    {
        // Arrange
        var mouseStatePressedOrigin = new MouseState(
            _mouseInput.DragVariance,
            _mouseInput.DragVariance,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressedOrigin);
        _mouseHandler.ClearReceivedCalls();

        var mouseStateWithinDragDropToleranceMax = new MouseState(
            _mouseInput.DragVariance + _mouseInput.DragVariance,
            _mouseInput.DragVariance + _mouseInput.DragVariance,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateWithinDragDropToleranceMax);

        var mouseStateWithinDragDropToleranceMin = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateWithinDragDropToleranceMin);

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);

        // Act
        _mouseInput.Poll(mouseStateReleased);

        // Assert
        _mouseHandler.Received().HandleMiddleMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.Received().HandleMiddleMouseClick(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.DidNotReceive().HandleMiddleMouseDragDone(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseDragging(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMouseMoving(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleMiddleMouseDoubleClick_Immediately_On_Second_Click_And_Suppress_MouseUp()
    {
        // Arrange
        var mouseStatePressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressed);

        var mouseStateFirstRelease = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateFirstRelease);

        var mouseStateSecondClick = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.Elapsed = new TimeSpan(0, 0, 0, 0, _mouseInput.DoubleClickDetectionTimeDelay);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.Poll(mouseStateSecondClick);

        // Assert
        _mouseHandler.Received().HandleMiddleMouseDoubleClick(Arg.Is(mouseStateSecondClick), Arg.Is(mouseStatePressed));
        _mouseHandler.DidNotReceive().HandleMiddleMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseDown(Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());

        // Act - Releasing
        _mouseHandler.ClearReceivedCalls();

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateReleased);

        // Assert - Releasing
        _mouseHandler.DidNotReceive().HandleMiddleMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseDown(Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }
    
    [TestMethod]
    public void MouseInput_Should_Not_Call_HandleMiddleMouseDoubleClick_If_ResetDoubleClickDetection_Is_Called()
    {
        // Arrange
        var mouseStatePressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressed);

        var mouseStateFirstRelease = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateFirstRelease);

        var mouseStateSecondClick = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.Elapsed = new TimeSpan(0, 0, 0, 0, _mouseInput.DoubleClickDetectionTimeDelay);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.ResetDoubleClickDetection();
        _mouseInput.Poll(mouseStateSecondClick);

        // Assert
        _mouseHandler.Received().HandleMiddleMouseDown(Arg.Is(mouseStateSecondClick));
        _mouseHandler.DidNotReceive().HandleMiddleMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());

        // Act - Releasing
        _mouseHandler.ClearReceivedCalls();

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateReleased);

        // Assert - Releasing
        _mouseHandler.DidNotReceive().HandleMiddleMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseDown(Arg.Any<MouseState>());
        _mouseHandler.Received().HandleMiddleMouseClick(Arg.Is(mouseStateReleased), Arg.Is(mouseStateSecondClick));
        _mouseHandler.Received().HandleMiddleMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStateSecondClick));
    }

    [TestMethod]
    public void MouseInput_Should_Not_Call_HandleMiddleMouseDoubleClick_If_DoubleClickDetection_Time_Has_Passed()
    {
        // Arrange
        var mouseStatePressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressed);

        var mouseStateFirstRelease = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateFirstRelease);

        var mouseStateSecondClick = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.Elapsed = new TimeSpan(0, 0, 0, 0, _mouseInput.DoubleClickDetectionTimeDelay + 1);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.Poll(mouseStateSecondClick);

        // Assert
        _mouseHandler.Received().HandleMiddleMouseDown(Arg.Is(mouseStateSecondClick));

        _mouseHandler.DidNotReceive().HandleMiddleMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());

        // Act - Releasing
        _mouseHandler.ClearReceivedCalls();

        var mouseStateReleased = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseInput.Poll(mouseStateReleased);

        // Assert - Releasing
        _mouseHandler.Received().HandleMiddleMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStateSecondClick));
        _mouseHandler.Received().HandleMiddleMouseClick(Arg.Is(mouseStateReleased), Arg.Is(mouseStateSecondClick));
        _mouseHandler.DidNotReceive().HandleMiddleMouseDragDone(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseDragging(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMouseMoving(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseDoubleClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [DataTestMethod]
    [DataRow(-1)]
    [DataRow(1)]
    public void MouseInput_Should_Call_HandleMiddleMouseDragging_When_Dragging_With_Middle_Down(int varianceMultiplier)
    {
        // Arrange
        var startingPosition = _mouseInput.DragVariance * 2;

        var mouseStatePressedOrigin = new MouseState(
            startingPosition,
            startingPosition,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressedOrigin);
        _mouseHandler.ClearReceivedCalls();

        var mouseStateDragging = new MouseState(
            startingPosition + (_mouseInput.DragVariance + 1) * varianceMultiplier,
            startingPosition + (_mouseInput.DragVariance + 1) * varianceMultiplier,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);

        // Act
        _mouseInput.Poll(mouseStateDragging);

        // Assert
        _mouseHandler.Received().HandleMiddleMouseDragging(Arg.Is(mouseStateDragging), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.DidNotReceive().HandleMouseMoving(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        _mouseHandler.DidNotReceive().HandleMiddleMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }

    [TestMethod]
    public void MouseInput_Should_Call_HandleMiddleMouseDragDone_And_Middle_Up_When_Releasing_Middle_After_Drag()
    {
        // Arrange
        var mouseStatePressedOrigin = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _mouseInput.Poll(mouseStatePressedOrigin);

        var mouseStateDragging = new MouseState(
            _mouseInput.DragVariance + 1,
            _mouseInput.DragVariance + 1,
            0,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);

        _mouseInput.Poll(mouseStateDragging);

        var mouseStateReleased = new MouseState(
            _mouseInput.DragVariance + 1,
            _mouseInput.DragVariance + 1,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );

        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _mouseHandler.ClearReceivedCalls();

        // Act
        _mouseInput.Poll(mouseStateReleased);

        // Assert
        _mouseHandler.Received().HandleMiddleMouseUp(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.Received().HandleMiddleMouseDragDone(Arg.Is(mouseStateReleased), Arg.Is(mouseStatePressedOrigin));
        _mouseHandler.DidNotReceive().HandleMiddleMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }
    
    [TestMethod]
    public void Should_Not_Send_Event_Before_Neutral_State_When_WaitForNeutralStateBeforeApplyingNewSubscriptions_Is_True_For_New_Subscriptions()
    {
        // Arrange
        var secondMouseHandler = Substitute.For<IMouseHandler>();

        _mouseInput.WaitForNeutralStateBeforeApplyingNewSubscriptions = true;
            
        var mouseStateLeftPressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );
        
        _mouseInput.Poll(mouseStateLeftPressed);

        // Act
        _mouseInput.Subscribe(secondMouseHandler);
        _mouseInput.Unsubscribe(_mouseHandler);
        
        var mouseStateNeutral = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );
        
        _mouseInput.Poll(mouseStateNeutral);

        // Assert
        _mouseHandler.DidNotReceive().HandleLeftMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        secondMouseHandler.DidNotReceive().HandleLeftMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        secondMouseHandler.DidNotReceive().HandleLeftMouseUp(Arg.Any<MouseState>(), Arg.Any<MouseState>());
    }
    
    [TestMethod]
    public void Should_Send_Event_Before_Neutral_State_When_WaitForNeutralStateBeforeApplyingNewSubscriptions_Is_False_For_New_Subscriptions()
    {
        // Arrange
        var secondMouseHandler = Substitute.For<IMouseHandler>();

        _mouseInput.WaitForNeutralStateBeforeApplyingNewSubscriptions = false;
            
        var mouseStateLeftPressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );
        
        _mouseInput.Poll(mouseStateLeftPressed);

        // Act
        _mouseInput.Subscribe(secondMouseHandler);
        _mouseInput.Unsubscribe(_mouseHandler);
        
        var mouseStateNeutral = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );
        
        _mouseInput.Poll(mouseStateNeutral);

        // Assert
        _mouseHandler.DidNotReceive().HandleLeftMouseClick(Arg.Any<MouseState>(), Arg.Any<MouseState>());
        secondMouseHandler.Received().HandleLeftMouseClick(Arg.Is(mouseStateNeutral), Arg.Is(mouseStateLeftPressed));
        secondMouseHandler.Received().HandleLeftMouseUp(Arg.Is(mouseStateNeutral), Arg.Is(mouseStateLeftPressed));
    }
    
    [TestMethod]
    public void Should_Send_Event_After_Neutral_State_Achieved_When_WaitForNeutralStateBeforeApplyingNewSubscriptions_Is_True()
    {
        // Arrange
        var secondMouseHandler = Substitute.For<IMouseHandler>();

        _mouseInput.WaitForNeutralStateBeforeApplyingNewSubscriptions = true;
            
        var mouseStateLeftPressed = new MouseState(
            0,
            0,
            0,
            ButtonState.Pressed,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );
        
        _mouseInput.Poll(mouseStateLeftPressed);

        _mouseInput.Subscribe(secondMouseHandler);
        _mouseInput.Unsubscribe(_mouseHandler);
        
        var mouseStateNeutral = new MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released
        );
        
        _mouseInput.Poll(mouseStateNeutral);
        
        // The WaitForNeutralStateBeforeApplyingNewSubscriptions feature does not automatically reset state related to detecting double clicks. 
        // Handlers must call ResetDoubleClickDetection manually if they need to suppress double click events going to another handler.
        _mouseInput.ResetDoubleClickDetection();

        // Act
        _mouseInput.Poll(mouseStateLeftPressed);

        // Assert
        secondMouseHandler
            .Received()
            .HandleLeftMouseDown(Arg.Is(mouseStateLeftPressed));
    }
}