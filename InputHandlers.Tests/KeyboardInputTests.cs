using System;
using System.Linq;

using InputHandlers.Keyboard;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Input;

using NSubstitute;

using KeyboardInput = InputHandlers.Keyboard.KeyboardInput;

namespace InputHandlers.Tests
{
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
            _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());

            secondKeyboardHandler
                .Received()
                .HandleKeyboardKeyDown(
                    Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                    Arg.Is(Keys.A),
                    Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
                );
        }

        [TestMethod]
        public void KeyboardInput_Should_Call_No_Handlers_When_Polling_With_Blank_KeyboardState()
        {
            // Arrange
            var keyboardState = new KeyboardState(new Keys [0]);

            // Act
            _keyboardInput.Poll(keyboardState);

            // Assert
            _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
            _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
            _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
            _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
        }

        [TestMethod]
        public void MouseInput_Should_Increment_UpdateNumber_On_Poll()
        {
            // Arrange
            var secondMouseHandler = Substitute.For<IKeyboardHandler>();

            _keyboardInput.Subscribe(secondMouseHandler);

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
        [DataRow(Keys.A, false)]
        [DataRow(Keys.NumPad0, true)]
        [DataRow(Keys.LeftShift, true)]
        [DataRow(Keys.RightShift, true)]
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

        [TestMethod]
        public void KeyboardInput_Should_Call_HandleKeyboardKeysReleased_When_Key_Is_Released()
        {
            // Arrange
            var keyboardState = new KeyboardState(Keys.A);

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

        [TestMethod]
        public void KeyboardInput_Should_Call_HandleKeyboardKeyDown_With_Keys_Pressed_In_Collection_And_Focus_Key_For_Key_Last_Pressed()
        {
            // Arrange
            var keyboardStateA = new KeyboardState(Keys.A);
            _keyboardInput.Poll(keyboardStateA);

            _keyboardHandler.ClearReceivedCalls();

            var keyboardStateB = new KeyboardState(Keys.A, Keys.B);

            // Act
            _keyboardInput.Poll(keyboardStateB);

            // Assert
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

        [TestMethod]
        public void KeyboardInput_Should_Call_HandleKeyboardKeyDown_If_Multiple_Keys_Down_At_Same_Time()
        {
            // Arrange
            var keyboardStateAB = new KeyboardState(Keys.A, Keys.B);

            // Act
            _keyboardInput.Poll(keyboardStateAB);

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

        [TestMethod]
        public void Keys_Are_Odered_In_Enum_Order_From_Xna_When_Returned_In_An_Event()
        {
            // Arrange
            var keyboardStateBA = new KeyboardState(Keys.B, Keys.A);

            // Act
            _keyboardInput.Poll(keyboardStateBA);

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

        [TestMethod]
        public void KeyboardInput_Should_Call_HandleKeyboardKeyDown_With_No_Lost_Event_For_Each_Changed_Key_If_List_Of_Keys_Changed()
        {
            // Arrange
            var keyboardStateA = new KeyboardState(Keys.A);
            _keyboardInput.Poll(keyboardStateA);

            _keyboardHandler.ClearReceivedCalls();

            var keyboardStateBC = new KeyboardState(Keys.B, Keys.C);

            // Act
            _keyboardInput.Poll(keyboardStateBC);

            // Assert
            _keyboardHandler
                .Received()
                .HandleKeyboardKeyDown(
                    Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.B, Keys.C)),
                    Arg.Is(Keys.B),
                    Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
                );

            _keyboardHandler
                .Received()
                .HandleKeyboardKeyDown(
                    Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.B, Keys.C)),
                    Arg.Is(Keys.C),
                    Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
                );

            _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
            _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
            _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
        }

        [TestMethod]
        public void KeyboardInput_Should_Call_HandleKeyboardKeyLost_For_Key_Last_Released()
        {
            // Arrange
            var keyboardStateA = new KeyboardState(Keys.A);
            _keyboardInput.Poll(keyboardStateA);

            var keyboardStateAB = new KeyboardState(Keys.A, Keys.B);

            _keyboardInput.Poll(keyboardStateAB);

            _keyboardHandler.ClearReceivedCalls();

            var keyboardStateB = new KeyboardState(Keys.A);
            
            // Act
            _keyboardInput.Poll(keyboardStateB);

            // Assert
            _keyboardHandler
                .Received()
                .HandleKeyboardKeyLost(
                    Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A)),
                    Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
                    );

            _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
            _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
            _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
        }

        [TestMethod]
        public void KeyboardInput_Should_Only_Call_HandleKeyboardKeyDown_Once_For_Same_State()
        {
            // Arrange
            var keyboardState = new KeyboardState(Keys.A);
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

        [TestMethod]
        public void KeyboardInput_Should_Call_HandleKeyboardKeyRepeat_When_Same_Keyboard_State_Is_Present_After_Delay()
        {
            // Arrange
            var keyboardState = new KeyboardState(Keys.A);
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
                    Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
                );
        }

        [TestMethod]
        public void KeyboardInput_Not_Should_Call_HandleKeyboardKeyRepeat_When_Same_Keyboard_State_Is_Present_After_First_Repeat_And_Before_First_RepeatFrequency()
        {
            // Arrange
            var keyboardState = new KeyboardState(Keys.A);
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

        [TestMethod]
        public void KeyboardInput_Should_Call_HandleKeyboardKeyRepeat_On_First_RepeatFrequency_When_Repeating()
        {
            // Arrange
            var keyboardState = new KeyboardState(Keys.A);
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
                    Arg.Is(Keys.A),
                    Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
                );

            _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
            _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
            _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
        }

        [TestMethod]
        public void KeyboardInput_Should_Not_Call_HandleKeyboardKeyRepeat_After_First_RepeatFrequency_And_Before_Second_RepeatFrequency_When_Repeating()
        {
            // Arrange
            var keyboardState = new KeyboardState(Keys.A);
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

        [TestMethod]
        public void KeyboardInput_Should_Call_HandleKeyboardKeyRepeat_On_Second_RepeatFrequency_When_Repeating()
        {
            // Arrange
            var keyboardState = new KeyboardState(Keys.A);
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
                    Arg.Is(Keys.A),
                    Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
                );

            _keyboardHandler.DidNotReceive().HandleKeyboardKeyDown(Arg.Any<Keys[]>(), Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
            _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
            _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
        }

        [TestMethod]
        public void KeyboardInput_Should_Call_HandleKeyboardKeysReleased_If_No_Keys_While_Repeating()
        {
            // Arrange
            var keyboardState = new KeyboardState(Keys.A);
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

        [TestMethod]
        public void KeyboardInput_Should_Not_Call_HandleKeyboardKeyLost_And_Not_Call_Other_Events_When_Keys_Released_And_Keys_Still_Down()
        {
            // Arrange
            var keyboardState = new KeyboardState(Keys.A, Keys.B, Keys.C);
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

        [TestMethod]
        public void KeyboardInput_Should_Not_Call_Other_Events_When_Keys_Released_And_Keys_Still_Down_On_Second_Poll()
        {
            // Arrange
            var keyboardState = new KeyboardState(Keys.A, Keys.B, Keys.C);
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

        [TestMethod]
        public void KeyboardInput_Should_Send_KeyDown_For_Only_New_Keys_When_Key_Lost_But_Some_Keys_Still_Down()
        {
            // Arrange
            var keyboardState = new KeyboardState(Keys.A, Keys.B, Keys.C);
            _keyboardInput.Poll(keyboardState);

            var keyboardStateReleased = new KeyboardState(Keys.A, Keys.B);
            _keyboardInput.Poll(keyboardStateReleased);

            var keyboardStateNewKeysDown = new KeyboardState(Keys.A, Keys.B, Keys.D, Keys.E);

            _keyboardHandler.ClearReceivedCalls();

            // Act
            _keyboardInput.Poll(keyboardStateNewKeysDown);

            // Assert
            _keyboardHandler
                .Received()
                .HandleKeyboardKeyDown(
                    Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A, Keys.B, Keys.D, Keys.E)),
                    Arg.Is(Keys.D),
                    Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
                );

            _keyboardHandler
                .Received()
                .HandleKeyboardKeyDown(
                    Arg.Is<Keys[]>(k => AssertKeysMatch(k, Keys.A, Keys.B, Keys.D, Keys.E)),
                    Arg.Is(Keys.E),
                    Arg.Is<KeyboardModifier>(k => k == KeyboardModifier.None)
                );

            _keyboardHandler.DidNotReceive().HandleKeyboardKeyLost(Arg.Any<Keys[]>(), Arg.Any<KeyboardModifier>());
            _keyboardHandler.DidNotReceive().HandleKeyboardKeyRepeat(Arg.Any<Keys>(), Arg.Any<KeyboardModifier>());
            _keyboardHandler.DidNotReceive().HandleKeyboardKeysReleased();
        }

        [TestMethod]
        public void KeyboardInput_Should_Prioritise_KeyDown_State_Over_KeyLost_State_When_Keys_Lost_And_New_Key_Down_Happen_At_Same_Time()
        {
            // Arrange
            var keyboardState = new KeyboardState(Keys.A, Keys.B, Keys.C);
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

        private bool AssertKeysMatch(Keys[] actual, params Keys[] expected)
        {
            CollectionAssert.AreEqual(expected, actual);
            return true;
        }
    }
}
