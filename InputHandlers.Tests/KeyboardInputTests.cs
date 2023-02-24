using System;
using System.Linq;

using InputHandlers.Keyboard;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Input;

using NSubstitute;
using NSubstitute.ClearExtensions;

using KeyboardInput = InputHandlers.Keyboard.KeyboardInput;

namespace InputHandlers.Tests;

[TestClass]
public class KeyboardInputTests
{
    private KeyboardInput _keyboardInput;
    private IKeyboardHandler _keyboardHandler;
    private TestStopwatchProvider _testStopwatchProvider;

    [TestInitialize]
    public void Setup()
    {
        _testStopwatchProvider = new TestStopwatchProvider();
        _keyboardHandler = Substitute.For<IKeyboardHandler>();
        _keyboardInput = new KeyboardInput(_testStopwatchProvider);

        _keyboardInput.Subscribe(_keyboardHandler);
    }

    [TestMethod]
    public void KeyboardInput_Should_Subscribe_And_Unsubscribe_Correctly()
    {
        // Arrange
        var secondKeyboardHandler = Substitute.For<IKeyboardHandler>();

        _keyboardInput.Subscribe(secondKeyboardHandler);
        _keyboardInput.Unsubscribe(_keyboardHandler);

        var keyboardState = new KeyboardState(Keys.A);

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .DidNotReceive()
            .HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());

        secondKeyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );
    }
    
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public void Remove_Subscription_When_Same_Handler_Is_Also_In_Pending_Add_And_Remove_Timestamp_Is_Greater_Or_Equal_Then_Subscription_Is_Removed(int advanceTime)
    {
        // Arrange
        var secondKeyboardHandler = Substitute.For<IKeyboardHandler>();

        _keyboardInput.Unsubscribe(_keyboardHandler);
        _keyboardInput.Subscribe(secondKeyboardHandler);
        _testStopwatchProvider.AdvanceByMilliseconds(advanceTime);
        _keyboardInput.Unsubscribe(secondKeyboardHandler);

        var keyboardState = new KeyboardState(Keys.A);

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .DidNotReceive()
            .HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());

        secondKeyboardHandler
            .DidNotReceive()
            .HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
    }
    
    [TestMethod]
    public void Remove_Subscription_When_Same_Handler_Is_Also_In_Pending_Add_And_Remove_Timestamp_Is_Less_Then_Subscription_Is_Added()
    {
        // Arrange
        var secondKeyboardHandler = Substitute.For<IKeyboardHandler>();

        _keyboardInput.Unsubscribe(_keyboardHandler);
        _keyboardInput.Subscribe(secondKeyboardHandler);
        _keyboardInput.Unsubscribe(secondKeyboardHandler);
        _testStopwatchProvider.AdvanceByMilliseconds(1);
        _keyboardInput.Subscribe(secondKeyboardHandler);

        var keyboardState = new KeyboardState(Keys.A);

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .DidNotReceive()
            .HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());

        secondKeyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );
    }

    [TestMethod]
    public void KeyboardInput_Can_Subscribe_While_Within_Handler_Of_Another_Subscription()
    {
        // Arrange
        var secondKeyboardHandler = Substitute.For<IKeyboardHandler>();

        _keyboardHandler
            .When(k => k.HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>()))
            .Do(ci => _keyboardInput.Subscribe(secondKeyboardHandler));

        var keyboardState = new KeyboardState(Keys.A);

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        secondKeyboardHandler
            .DidNotReceive()
            .HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());

        // Arrange - second handler should now be subscribed
        _keyboardHandler.ClearSubstitute();
        keyboardState = new KeyboardState(Keys.B);
            
        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.B)),
                Arg.Is(Keys.B),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        secondKeyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.B)),
                Arg.Is(Keys.B),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );
    }

    [TestMethod]
    public void KeyboardInput_Can_Unsubscribe_While_Within_Handler_Of_Another_Subscription()
    {
        // Arrange
        var secondKeyboardHandler = Substitute.For<IKeyboardHandler>();

        _keyboardInput.Subscribe(secondKeyboardHandler);

        _keyboardHandler
            .When(k => k.HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>()))
            .Do(ci => _keyboardInput.Unsubscribe(secondKeyboardHandler));

        var keyboardState = new KeyboardState(Keys.A);

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        secondKeyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        // Arrange - second handler should now be unsubscribed
        _keyboardHandler.ClearSubstitute();
        secondKeyboardHandler.ClearReceivedCalls();

        keyboardState = new KeyboardState(Keys.B);

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.B)),
                Arg.Is(Keys.B),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        secondKeyboardHandler
            .DidNotReceive()
            .HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
    }

    [TestMethod]
    public void KeyboardInput_Should_Ignore_Second_Subscription_Of_Same_Handler()
    {
        // Arrange
        _keyboardInput.Subscribe(_keyboardHandler);
        var keyboardState = new KeyboardState(Keys.A);

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        Assert.AreEqual(1, _keyboardHandler.ReceivedCalls().Count());

        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [TestMethod]
    public void KeyboardInput_Should_Call_No_Handlers_When_Polling_With_Blank_KeyboardState()
    {
        // Arrange
        var keyboardState = new KeyboardState(Array.Empty<Keys>());

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [TestMethod]
    public void KeyboardInput_Should_Increment_UpdateNumber_On_Poll()
    {
        // Arrange
        var secondKeyboardHandler = Substitute.For<IKeyboardHandler>();

        _keyboardInput.Subscribe(secondKeyboardHandler);

        var keyboardState = new KeyboardState();

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        Assert.AreEqual(1, _keyboardInput.UpdateNumber);

        // Act - poll 2
        _keyboardInput.Poll(keyboardState);

        // Assert = poll 2
        Assert.AreEqual(2, _keyboardInput.UpdateNumber);
    }

    [TestMethod]
    public void KeyboardInput_Should_Reset_To_No_State_And_Set_UpdateNumber_To_Zero_When_Resetting()
    {
        // Arrange
        var keyboardState = new KeyboardState(Keys.A);

        _keyboardInput.Poll(keyboardState);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Reset();

        // Assert
        Assert.AreEqual(0, _keyboardInput.UpdateNumber);

        // Act - poll again with keys released, state is nothing and so no keyboard up will occur
        var keyboardStateReleased = new KeyboardState();

        _keyboardInput.Poll(keyboardStateReleased);

        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [TestMethod]
    public void KeyboardInput_Should_Broadcast_To_Both_Handlers_When_KeyboardInput_Has_Two_Subscriptions()
    {
        // Arrange
        var secondKeyboardHandler = Substitute.For<IKeyboardHandler>();

        _keyboardInput.Subscribe(secondKeyboardHandler);

        var keyboardState = new KeyboardState(Keys.A);

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        secondKeyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.F1, false)]
    [DataRow(Keys.NumPad0, true)]
    [DataRow(Keys.LeftShift, true)]
    [DataRow(Keys.RightShift, true)]
    [DataRow(Keys.LeftAlt, true)]
    [DataRow(Keys.RightAlt, true)]
    [DataRow(Keys.LeftControl, true)]
    [DataRow(Keys.RightControl, true)]
    public void KeyboardInput_Should_Call_HandleKeyboardKeyDown_When_Key_Is_Pressed(Keys key, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;
        var keyboardState = new KeyboardState(key);

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, key)),
                Arg.Is(key),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.F1, false)]
    [DataRow(Keys.NumPad0, true)]
    [DataRow(Keys.LeftShift, true)]
    [DataRow(Keys.RightShift, true)]
    [DataRow(Keys.LeftAlt, true)]
    [DataRow(Keys.RightAlt, true)]
    [DataRow(Keys.LeftControl, true)]
    [DataRow(Keys.RightControl, true)]
    public void KeyboardInput_Should_Call_HandleKeyboardKeysReleased_When_Key_Is_Released(Keys key, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;
        var keyboardState = new KeyboardState(key);

        _keyboardInput.Poll(keyboardState);

        var keyboardStateReleased = new KeyboardState();

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateReleased);

        // Assert
        _keyboardHandler.Received().HandleKeyboardKeysReleased();
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
    }

    [DataTestMethod]
    [DataRow(Keys.X, Keys.A, false)]
    [DataRow(Keys.F1, Keys.CapsLock, false)]
    [DataRow(Keys.D0, Keys.NumPad0, true)]
    [DataRow(Keys.RightAlt, Keys.LeftShift, true)]
    [DataRow(Keys.X, Keys.RightShift, true)]
    [DataRow(Keys.LeftControl, Keys.X, true)]
    public void KeyboardInput_Should_Call_HandleKeyboardKeyDown_With_Keys_Pressed_In_Collection_And_Focus_Key_For_Key_Last_Pressed(Keys oldKey, Keys newKey, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardStateOldKey = new KeyboardState(oldKey);
        _keyboardInput.Poll(keyboardStateOldKey);

        _keyboardHandler.ClearReceivedCalls();

        var keyboardStateNewKey = new KeyboardState(oldKey, newKey);

        // Act
        _keyboardInput.Poll(keyboardStateNewKey);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, oldKey, newKey)),
                Arg.Is(newKey),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, Keys.A, false)]
    [DataRow(Keys.F1, Keys.CapsLock, false)]
    [DataRow(Keys.D0, Keys.NumPad0, true)]
    [DataRow(Keys.RightAlt, Keys.LeftShift, true)]
    [DataRow(Keys.X, Keys.RightShift, true)]
    [DataRow(Keys.LeftControl, Keys.X, true)]
    public void KeyboardInput_Should_Call_HandleKeyboardKeyDown_If_Multiple_Keys_Down_At_Same_Time(Keys key1, Keys key2, bool treatModifiersAsKeys)
    {
        // Arrange
        var keyboardState = new KeyboardState(Keys.A, Keys.B);

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A, Keys.B)),
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A, Keys.B)),
                Arg.Is(Keys.B),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.A, Keys.B, Keys.C, false)]
    [DataRow(Keys.F1, Keys.CapsLock, Keys.Escape, false)]
    [DataRow(Keys.D0, Keys.NumPad0, Keys.PageUp, true)]
    [DataRow(Keys.RightAlt, Keys.LeftShift, Keys.RightControl, true)]
    [DataRow(Keys.A, Keys.LeftShift, Keys.RightControl, true)]
    [DataRow(Keys.RightAlt, Keys.A, Keys.RightControl, true)]
    [DataRow(Keys.RightAlt, Keys.LeftShift, Keys.A, true)]
    public void KeyboardInput_Should_Call_HandleKeyboardKeyDown_With_No_Lost_Event_For_Each_Changed_Key_If_List_Of_Keys_Changed(Keys oldKey, Keys newKey1, Keys newKey2, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardStateA = new KeyboardState(oldKey);
        _keyboardInput.Poll(keyboardStateA);

        _keyboardHandler.ClearReceivedCalls();

        var keyboardStateChangedKeys = new KeyboardState(newKey1, newKey2);

        // Act
        _keyboardInput.Poll(keyboardStateChangedKeys);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, newKey1, newKey2)),
                Arg.Is(newKey1),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, newKey1, newKey2)),
                Arg.Is(newKey2),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler
            .DidNotReceive()
            .HandleKeyboardKeyDown(
                Arg.Any<Keys[]>(),
                Arg.Is(oldKey),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, Keys.A, false)]
    [DataRow(Keys.F1, Keys.BrowserBack, false)]
    [DataRow(Keys.D0, Keys.NumPad0, true)]
    [DataRow(Keys.RightAlt, Keys.LeftShift, true)]
    [DataRow(Keys.X, Keys.RightShift, true)]
    [DataRow(Keys.LeftControl, Keys.X, true)]
    public void KeyboardInput_Should_Call_HandleKeyboardKeyLost_For_Key_Last_Released(Keys remainingKey, Keys lostKey, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(remainingKey);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateExtraKey = new KeyboardState(remainingKey, lostKey);

        _keyboardInput.Poll(keyboardStateExtraKey);

        var keyboardStateKeyLost = new KeyboardState(remainingKey);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateKeyLost);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyLost(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, remainingKey)),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.F1, false)]
    [DataRow(Keys.NumPad0, true)]
    [DataRow(Keys.LeftShift, true)]
    [DataRow(Keys.RightShift, true)]
    [DataRow(Keys.LeftAlt, true)]
    [DataRow(Keys.RightAlt, true)]
    [DataRow(Keys.LeftControl, true)]
    [DataRow(Keys.RightControl, true)]
    public void KeyboardInput_Should_Only_Call_HandleKeyboardKeyDown_Once_For_Same_State(Keys key, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(key);
        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay - 1);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.F1, false)]
    [DataRow(Keys.NumPad0, true)]
    [DataRow(Keys.LeftShift, true)]
    [DataRow(Keys.RightShift, true)]
    [DataRow(Keys.LeftAlt, true)]
    [DataRow(Keys.RightAlt, true)]
    [DataRow(Keys.LeftControl, true)]
    [DataRow(Keys.RightControl, true)]
    public void KeyboardInput_Should_Call_HandleKeyboardKeyRepeat_When_Same_Keyboard_State_Is_Present_After_Delay(Keys key, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(key);
        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);

        _keyboardHandler.ClearReceivedCalls();
            
        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyRepeat(
                Arg.Is(key),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );
    }

    [DataTestMethod]
    [DataRow(Keys.X, Keys.A, false)]
    [DataRow(Keys.F1, Keys.BrowserBack, false)]
    [DataRow(Keys.D0, Keys.NumPad0, true)]
    [DataRow(Keys.RightAlt, Keys.LeftShift, true)]
    [DataRow(Keys.X, Keys.RightShift, true)]
    [DataRow(Keys.LeftControl, Keys.X, true)]
    public void KeyboardInput_Should_Not_Call_HandleKeyboardKeyRepeat_When_Newest_Key_Is_Lost(Keys oldKey, Keys newKey, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(oldKey);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateExtraKey = new KeyboardState(oldKey, newKey);

        _keyboardInput.Poll(keyboardStateExtraKey);

        var keyboardStateKeyLost = new KeyboardState(oldKey);

        _keyboardInput.Poll(keyboardStateKeyLost);

        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateKeyLost);

        // Assert
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, Keys.A, false)]
    [DataRow(Keys.F1, Keys.BrowserBack, false)]
    [DataRow(Keys.D0, Keys.NumPad0, true)]
    [DataRow(Keys.RightAlt, Keys.LeftShift, true)]
    [DataRow(Keys.X, Keys.RightShift, true)]
    [DataRow(Keys.LeftControl, Keys.X, true)]
    public void KeyboardInput_Should_Call_HandleKeyboardKeyRepeat_When_An_Older_Key_Is_Lost(Keys oldKey, Keys newKey, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(oldKey);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateExtraKey = new KeyboardState(oldKey, newKey);

        _keyboardInput.Poll(keyboardStateExtraKey);

        var keyboardStateKeyLost = new KeyboardState(newKey);

        _keyboardInput.Poll(keyboardStateKeyLost);

        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateKeyLost);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyRepeat(
                Arg.Is<Keys>(k => k == newKey),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }


    [DataTestMethod]
    [DataRow(Keys.X, Keys.A, false)]
    [DataRow(Keys.F1, Keys.BrowserBack, false)]
    [DataRow(Keys.D0, Keys.NumPad0, true)]
    [DataRow(Keys.RightAlt, Keys.LeftShift, true)]
    [DataRow(Keys.X, Keys.RightShift, true)]
    [DataRow(Keys.LeftControl, Keys.X, true)]
    public void KeyboardInput_Should_Call_HandleKeyboardKeyRepeat_When_An_Older_Key_Is_Lost_And_New_Key_Pressed_Again(Keys oldKey, Keys newKey, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(oldKey);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateExtraKey = new KeyboardState(oldKey, newKey);

        _keyboardInput.Poll(keyboardStateExtraKey);

        var keyboardStateKeyLost = new KeyboardState(newKey);

        _keyboardInput.Poll(keyboardStateKeyLost);

        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);

        _keyboardInput.Poll(keyboardStateKeyLost);

        _keyboardInput.Poll(keyboardStateExtraKey);

        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateExtraKey);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyRepeat(
                Arg.Is<Keys>(k => k == oldKey),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.F1, false)]
    [DataRow(Keys.NumPad0, true)]
    [DataRow(Keys.LeftShift, true)]
    [DataRow(Keys.RightShift, true)]
    [DataRow(Keys.LeftAlt, true)]
    [DataRow(Keys.RightAlt, true)]
    [DataRow(Keys.LeftControl, true)]
    [DataRow(Keys.RightControl, true)]
    public void KeyboardInput_Not_Should_Call_HandleKeyboardKeyRepeat_When_Same_Keyboard_State_Is_Present_After_First_Repeat_And_Before_First_RepeatFrequency(Keys key, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(key);
        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);
        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatFrequency - 1);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.F1, false)]
    [DataRow(Keys.NumPad0, true)]
    [DataRow(Keys.LeftShift, true)]
    [DataRow(Keys.RightShift, true)]
    [DataRow(Keys.LeftAlt, true)]
    [DataRow(Keys.RightAlt, true)]
    [DataRow(Keys.LeftControl, true)]
    [DataRow(Keys.RightControl, true)]
    public void KeyboardInput_Should_Call_HandleKeyboardKeyRepeat_On_First_RepeatFrequency_When_Repeating(Keys key, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(key);
        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);
        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatFrequency);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyRepeat(
                Arg.Is(key),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.F1, false)]
    [DataRow(Keys.NumPad0, true)]
    [DataRow(Keys.LeftShift, true)]
    [DataRow(Keys.RightShift, true)]
    [DataRow(Keys.LeftAlt, true)]
    [DataRow(Keys.RightAlt, true)]
    [DataRow(Keys.LeftControl, true)]
    [DataRow(Keys.RightControl, true)]
    public void KeyboardInput_Should_Not_Call_HandleKeyboardKeyRepeat_After_First_RepeatFrequency_And_Before_Second_RepeatFrequency_When_Repeating(Keys key, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(key);

        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);
        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatFrequency);
        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatFrequency - 1);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.F1, false)]
    [DataRow(Keys.NumPad0, true)]
    [DataRow(Keys.LeftShift, true)]
    [DataRow(Keys.RightShift, true)]
    [DataRow(Keys.LeftAlt, true)]
    [DataRow(Keys.RightAlt, true)]
    [DataRow(Keys.LeftControl, true)]
    [DataRow(Keys.RightControl, true)]
    public void KeyboardInput_Should_Call_HandleKeyboardKeyRepeat_On_Second_RepeatFrequency_When_Repeating(Keys key, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(key);

        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);
        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatFrequency);
        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatFrequency);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyRepeat(
                Arg.Is(key),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.F1, false)]
    [DataRow(Keys.NumPad0, true)]
    [DataRow(Keys.LeftShift, true)]
    [DataRow(Keys.RightShift, true)]
    [DataRow(Keys.LeftAlt, true)]
    [DataRow(Keys.RightAlt, true)]
    [DataRow(Keys.LeftControl, true)]
    [DataRow(Keys.RightControl, true)]
    public void KeyboardInput_Should_Call_HandleKeyboardKeysReleased_If_No_Keys_While_Repeating(Keys key, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(key);
        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateReleased = new KeyboardState();

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateReleased);

        // Assert
        _keyboardHandler.Received().HandleKeyboardKeysReleased();

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.F1, false)]
    [DataRow(Keys.NumPad0, true)]
    [DataRow(Keys.LeftShift, true)]
    [DataRow(Keys.RightShift, true)]
    [DataRow(Keys.LeftAlt, true)]
    [DataRow(Keys.RightAlt, true)]
    [DataRow(Keys.LeftControl, true)]
    [DataRow(Keys.RightControl, true)]
    public void KeyboardInput_Should_Not_Call_HandleKeyboardKeyLost_And_Not_Call_Other_Events_When_Keys_Released_And_Keys_Still_Down(Keys key, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(Keys.A, Keys.B, key);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateReleased = new KeyboardState(Keys.A, Keys.B);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateReleased);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyLost(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A, Keys.B)),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.F1, false)]
    [DataRow(Keys.NumPad0, true)]
    [DataRow(Keys.LeftShift, true)]
    [DataRow(Keys.RightShift, true)]
    [DataRow(Keys.LeftAlt, true)]
    [DataRow(Keys.RightAlt, true)]
    [DataRow(Keys.LeftControl, true)]
    [DataRow(Keys.RightControl, true)]
    public void KeyboardInput_Should_Not_Call_Other_Events_When_Keys_Released_And_Keys_Still_Down_On_Second_Poll(Keys key, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(Keys.A, Keys.B, key);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateReleased = new KeyboardState(Keys.A, Keys.B);
        _keyboardInput.Poll(keyboardStateReleased);
        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateReleased);

        // Assert
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.F1, false)]
    [DataRow(Keys.NumPad0, true)]
    [DataRow(Keys.LeftShift, true)]
    [DataRow(Keys.RightShift, true)]
    [DataRow(Keys.LeftAlt, true)]
    [DataRow(Keys.RightAlt, true)]
    [DataRow(Keys.LeftControl, true)]
    [DataRow(Keys.RightControl, true)]
    public void KeyboardInput_Should_Send_KeyDown_For_Only_New_Keys_When_Key_Lost_But_Some_Keys_Still_Down(Keys key, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(key, Keys.B, Keys.C);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateReleased = new KeyboardState(key, Keys.B);
        _keyboardInput.Poll(keyboardStateReleased);

        var keyboardStateNewKeysDown = new KeyboardState(key, Keys.B, Keys.D, Keys.E);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateNewKeysDown);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, key, Keys.B, Keys.D, Keys.E)),
                Arg.Is(Keys.D),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, key, Keys.B, Keys.D, Keys.E)),
                Arg.Is(Keys.E),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler
            .DidNotReceive()
            .HandleKeyboardKeyDown(
                Arg.Any<Keys[]>(),
                Arg.Is(key),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler
            .DidNotReceive()
            .HandleKeyboardKeyDown(
                Arg.Any<Keys[]>(),
                Arg.Is(Keys.B),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.F1, false)]
    [DataRow(Keys.NumPad0, true)]
    [DataRow(Keys.LeftShift, true)]
    [DataRow(Keys.RightShift, true)]
    [DataRow(Keys.LeftAlt, true)]
    [DataRow(Keys.RightAlt, true)]
    [DataRow(Keys.LeftControl, true)]
    [DataRow(Keys.RightControl, true)]
    public void KeyboardInput_Should_Prioritise_KeyDown_State_Over_KeyLost_State_When_Keys_Lost_And_New_Key_Down_Happen_At_Same_Time(Keys key, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;

        var keyboardState = new KeyboardState(Keys.A, Keys.B, key);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateReleased = new KeyboardState(Keys.A, Keys.B);
        _keyboardInput.Poll(keyboardStateReleased);

        var keyboardStateNewKeysDownWithOneKeyLost = new KeyboardState(Keys.A, Keys.D, Keys.E);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateNewKeysDownWithOneKeyLost);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A, Keys.D, Keys.E)),
                Arg.Is(Keys.D),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A, Keys.D, Keys.E)),
                Arg.Is(Keys.E),
                Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.LeftControl)]
    [DataRow(Keys.RightControl)]
    [DataRow(Keys.LeftShift)]
    [DataRow(Keys.RightShift)]
    [DataRow(Keys.LeftAlt)]
    [DataRow(Keys.RightAlt)]
    public void KeyboardInput_Should_Do_Nothing_If_Key_Is_Modifier_And_No_Other_Keys_Pressed(Keys modifierKey)
    {
        // Act
        var keyboardStateDown = new KeyboardState(modifierKey);
        _keyboardInput.Poll(keyboardStateDown);

        var keyboardStateReleased = new KeyboardState();
        _keyboardInput.Poll(keyboardStateReleased);

        _keyboardInput.Poll(keyboardStateDown);

        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);

        _keyboardInput.Poll(keyboardStateDown);

        _keyboardInput.Poll(keyboardStateReleased);

        // Assert
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.X, true)]
    [DataRow(Keys.LeftShift, false)]
    [DataRow(Keys.LeftShift, true)]
    public void KeyboardInput_Should_Do_Nothing_If_Key_Is_Unmananged(Keys unmanagedKey, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;
        _keyboardInput.UnmanagedKeys.Add(unmanagedKey);

        // Act
        var keyboardStateDown = new KeyboardState(unmanagedKey);
        _keyboardInput.Poll(keyboardStateDown);

        var keyboardStateReleased = new KeyboardState();
        _keyboardInput.Poll(keyboardStateReleased);

        _keyboardInput.Poll(keyboardStateDown);

        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);

        _keyboardInput.Poll(keyboardStateDown);

        _keyboardInput.Poll(keyboardStateReleased);

        // Assert
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.X, true)]
    [DataRow(Keys.LeftShift, false)]
    [DataRow(Keys.LeftShift, true)]
    public void KeyboardInput_Should_Do_Nothing_If_Key_Is_Unmananged_When_A_Managed_Key_Is_Down(Keys unmanagedKey, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;
        _keyboardInput.UnmanagedKeys.Add(unmanagedKey);

        var keyboardState = new KeyboardState(Keys.A);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateWithUnmanaged = new KeyboardState(Keys.A, unmanagedKey);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateWithUnmanaged);

        // Assert
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.X, false)]
    [DataRow(Keys.X, true)]
    [DataRow(Keys.LeftShift, false)]
    [DataRow(Keys.LeftShift, true)]
    public void KeyboardInput_Should_Do_Nothing_If_Key_Is_Unmananged_When_A_Managed_Key_Is_Lost(Keys unmanagedKey, bool treatModifiersAsKeys)
    {
        // Arrange
        _keyboardInput.TreatModifiersAsKeys = treatModifiersAsKeys;
        _keyboardInput.UnmanagedKeys.Add(unmanagedKey);

        var keyboardState = new KeyboardState(Keys.A, Keys.B);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateLost = new KeyboardState(Keys.A);
        _keyboardInput.Poll(keyboardStateLost);
            
        var keyboardStateWithUnmanaged = new KeyboardState(Keys.A, unmanagedKey);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateWithUnmanaged);

        var keyboardStateLostUnmanaged = new KeyboardState(Keys.A);

        _keyboardInput.Poll(keyboardStateLostUnmanaged);

        _keyboardInput.Poll(keyboardStateWithUnmanaged);

        // Assert
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.LeftControl, KeyboardModifier.Ctrl)]
    [DataRow(Keys.RightControl, KeyboardModifier.Ctrl)]
    [DataRow(Keys.LeftShift, KeyboardModifier.Shift)]
    [DataRow(Keys.RightShift, KeyboardModifier.Shift)]
    [DataRow(Keys.LeftAlt, KeyboardModifier.Alt)]
    [DataRow(Keys.RightAlt, KeyboardModifier.Alt)]
    public void KeyboardInput_Should_Send_HandleKeyboardKeyDown_With_KeyboardModifier_Flags_Set_When_Not_Treating_Modifiers_As_Keys(Keys key, KeyboardModifier keyboardModifier)
    {
        // Arrange
        var keyboardState = new KeyboardState(key, Keys.A);

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == keyboardModifier)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [TestMethod]
    public void KeyboardInput_Should_Send_HandleKeyboardKeyDown_With_Multiple_KeyboardModifier_Flags_Set_When_Not_Treating_Modifiers_As_Keys()
    {
        // Arrange
        var keyboardState = new KeyboardState(Keys.LeftControl, Keys.RightShift, Keys.LeftAlt, Keys.A);

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == (KeyboardModifier.Alt | KeyboardModifier.Shift | KeyboardModifier.Ctrl))
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.LeftControl, KeyboardModifier.Ctrl)]
    [DataRow(Keys.RightControl, KeyboardModifier.Ctrl)]
    [DataRow(Keys.LeftShift, KeyboardModifier.Shift)]
    [DataRow(Keys.RightShift, KeyboardModifier.Shift)]
    [DataRow(Keys.LeftAlt, KeyboardModifier.Alt)]
    [DataRow(Keys.RightAlt, KeyboardModifier.Alt)]
    public void KeyboardInput_Should_Send_HandleKeyboardKeyDown_With_KeyboardModifier_Flags_Set_And_No_Focus_Key_When_Only_Modifiers_Change(Keys key, KeyboardModifier keyboardModifier)
    {
        // Arrange
        var keyboardState = new KeyboardState(Keys.A);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateWithModifiers = new KeyboardState(key, Keys.A);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateWithModifiers);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is(Keys.None),
                Arg.Is<KeyboardModifier>(k => k == keyboardModifier)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [TestMethod]
    public void KeyboardInput_Should_Send_HandleKeyboardKeyDown_With_Multiple_KeyboardModifier_Flags_Set_And_No_Focus_Key_When_Only_Modifiers_Change()
    {
        // Arrange
        var keyboardState = new KeyboardState(Keys.A);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateWithModifiers = new KeyboardState(Keys.LeftControl, Keys.LeftShift, Keys.RightAlt, Keys.A);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateWithModifiers);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyDown(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is(Keys.None),
                Arg.Is<KeyboardModifier>(k => k == (KeyboardModifier.Alt | KeyboardModifier.Shift | KeyboardModifier.Ctrl))
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.LeftControl, KeyboardModifier.Ctrl)]
    [DataRow(Keys.RightControl, KeyboardModifier.Ctrl)]
    [DataRow(Keys.LeftShift, KeyboardModifier.Shift)]
    [DataRow(Keys.RightShift, KeyboardModifier.Shift)]
    [DataRow(Keys.LeftAlt, KeyboardModifier.Alt)]
    [DataRow(Keys.RightAlt, KeyboardModifier.Alt)]
    public void KeyboardInput_Should_Send_HandleKeyboardKeyRepeat_With_KeyboardModifier_Flags_Set_When_Not_Treating_Modifiers_As_Keys(Keys key, KeyboardModifier keyboardModifier)
    {
        // Arrange
        var keyboardState = new KeyboardState(Keys.A, key);
        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyRepeat(
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == keyboardModifier)
            );
    }

    [TestMethod]
    public void KeyboardInput_Should_Send_HandleKeyboardKeyRepeat_With_Multiple_KeyboardModifier_Flags_Set_When_Not_Treating_Modifiers_As_Keys()
    {
        // Arrange
        var keyboardState = new KeyboardState(Keys.A, Keys.LeftControl, Keys.LeftShift, Keys.RightAlt);
        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardState);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyRepeat(
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == (KeyboardModifier.Alt | KeyboardModifier.Shift | KeyboardModifier.Ctrl))
            );
    }

    [DataTestMethod]
    [DataRow(Keys.LeftControl, KeyboardModifier.Ctrl)]
    [DataRow(Keys.RightControl, KeyboardModifier.Ctrl)]
    [DataRow(Keys.LeftShift, KeyboardModifier.Shift)]
    [DataRow(Keys.RightShift, KeyboardModifier.Shift)]
    [DataRow(Keys.LeftAlt, KeyboardModifier.Alt)]
    [DataRow(Keys.RightAlt, KeyboardModifier.Alt)]
    public void KeyboardInput_Should_Send_HandleKeyboardKeyLost_With_KeyboardModifier_Flags_Set_When_Not_Treating_Modifiers_As_Keys(Keys key, KeyboardModifier keyboardModifier)
    {
        // Arrange
        var keyboardState = new KeyboardState(Keys.A, key);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateExtraKey = new KeyboardState(Keys.A, Keys.B, key);

        _keyboardInput.Poll(keyboardStateExtraKey);

        var keyboardStateKeyLost = new KeyboardState(Keys.A, key);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateKeyLost);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyLost(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is<KeyboardModifier>(k => k == keyboardModifier)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [TestMethod]
    public void KeyboardInput_Should_Send_HandleKeyboardKeyLost_With_Multiple_KeyboardModifier_Flags_Set_When_Not_Treating_Modifiers_As_Keys()
    {
        // Arrange
        var keyboardState = new KeyboardState(Keys.A, Keys.LeftControl, Keys.LeftShift, Keys.RightAlt);
        _keyboardInput.Poll(keyboardState);

        var keyboardStateExtraKey = new KeyboardState(Keys.A, Keys.B, Keys.LeftControl, Keys.LeftShift, Keys.RightAlt);

        _keyboardInput.Poll(keyboardStateExtraKey);

        var keyboardStateKeyLost = new KeyboardState(Keys.A, Keys.LeftControl, Keys.LeftShift, Keys.RightAlt);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateKeyLost);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyLost(
                Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                Arg.Is<KeyboardModifier>(k => k == (KeyboardModifier.Alt | KeyboardModifier.Shift | KeyboardModifier.Ctrl))
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.LeftControl)]
    [DataRow(Keys.RightControl)]
    [DataRow(Keys.LeftShift)]
    [DataRow(Keys.RightShift)]
    [DataRow(Keys.LeftAlt)]
    [DataRow(Keys.RightAlt)]
    public void KeyboardInput_Should_Reset_Repeat_Timer_When_Modifiers_Change_While_Repeating(Keys key)
    {
        // Arrange
        var keyboardState = new KeyboardState(Keys.A);

        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay - 1);

        var keyboardStateWithModifier = new KeyboardState(Keys.A, key);
        _keyboardInput.Poll(keyboardStateWithModifier);

        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay - 1);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateWithModifier);

        // Assert
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }

    [DataTestMethod]
    [DataRow(Keys.LeftControl, KeyboardModifier.Ctrl)]
    [DataRow(Keys.RightControl, KeyboardModifier.Ctrl)]
    [DataRow(Keys.LeftShift, KeyboardModifier.Shift)]
    [DataRow(Keys.RightShift, KeyboardModifier.Shift)]
    [DataRow(Keys.LeftAlt, KeyboardModifier.Alt)]
    [DataRow(Keys.RightAlt, KeyboardModifier.Alt)]
    public void KeyboardInput_Should_Begin_Repeating_Again_With_Modifier_After_Resetting_Repeat_Timer_When_Modifiers_Change(Keys key, KeyboardModifier keyboardModifier)
    {
        // Arrange
        var keyboardState = new KeyboardState(Keys.A);

        _keyboardInput.Poll(keyboardState);
        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay - 1);

        var keyboardStateWithModifier = new KeyboardState(Keys.A, key);
        _keyboardInput.Poll(keyboardStateWithModifier);

        _testStopwatchProvider.AdvanceByMilliseconds(_keyboardInput.RepeatDelay);

        _keyboardHandler.ClearReceivedCalls();

        // Act
        _keyboardInput.Poll(keyboardStateWithModifier);

        // Assert
        _keyboardHandler
            .Received()
            .HandleKeyboardKeyRepeat(
                Arg.Is(Keys.A),
                Arg.Is<KeyboardModifier>(k => k == keyboardModifier)
            );

        _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
        _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
    }
    
    [TestMethod]
    public void WaitForNeutralStateBeforeApplyingNewSubscriptions()
    {
        Assert.Fail();
    } 

    private bool AssertKeysMatch(Keys[] actual, params Keys[] expected)
    {
        var expectedInEnumOrder = expected
            .OrderBy(x => x)
            .ToArray();

        CollectionAssert.AreEqual(expectedInEnumOrder, actual);
        return true;
    }
}