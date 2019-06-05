using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ByteCodeVM_Test : MonoBehaviour
{
    VM vm;

    // Init our VM
    void Start ()
    {
        vm = new VM();

        vm.Load(new byte[] {
                // Print Hello World
                (byte)VM.INST.PRINT_HELLO_WORLD,

                // Add(a,b) => push result,
                (byte)VM.INST.SET, (byte)2, // A
                (byte)VM.INST.SET, (byte)1, // B
                (byte)VM.INST.ADD,

                // Print float from 4 bytes (1234567 or 0x4996b438 or 49 96 b4 38)
                (byte)VM.INST.SET, 0x49,
                (byte)VM.INST.SET, 0x96,
                (byte)VM.INST.SET, 0xb4,
                (byte)VM.INST.SET, 0x38,
                (byte)VM.INST.PRINT_FLOAT,

                // Print float from 4 bytes WITHOUT USING STACK
                (byte)VM.INST.PRINT_FLOAT_NO_STACK, 0x49, 0x96, 0xb4, 0x38,

                // Wait using result from Add(a,b) since it's still in the stack
                (byte)VM.INST.WAIT,

                // Repeat
                (byte)VM.INST.REPEAT,
            });
    }

    // Loop through all instructions
    void Update ()
    {
        vm.Process(Time.time);
    }
}

public class VM
{
    // Max length of stack ( pushed values )
    private const int MAX_STACK = 128;

    // Our list of instructions / functions
    public enum INST: byte {
        PRINT_HELLO_WORLD,

        SET,
        ADD,

        PRINT_FLOAT,
        PRINT_FLOAT_NO_STACK,

        WAIT,
        REPEAT
    };

    // Stack ( stores our values for push/pop )
    private byte[] _stack;
    private int _stackSize;

    // Current index
    private int _curInstr;

    // Our bytecode
    private byte[] bytecode;

    // Wait until
    private float wait = -1;

    /// <summary>
    /// Load bytecode instructions
    /// </summary>
    /// <param name="bytecode">Bytecode</param>
    public void Load (byte[] bytecode)
    {
        this._stack = new byte[MAX_STACK];
        this._curInstr = 0;
        this.bytecode = bytecode;
    }

    /// <summary>
    /// Process instructions
    /// </summary>
    /// <param name="time">Current time</param>
    public void Process (float time)
    {
        // Do we need to wait?
        if (wait >= time) {
            Debug.Log($"Waiting {Mathf.Ceil(wait-time)} seconds...");
            return;
        }

        bool suspend = false; // Helper variable to break out of our while loop

        // Execute every byte until the end of suspend is true
        while (!suspend && _curInstr < bytecode.Length) {
            byte instruction = bytecode[_curInstr++];

            switch (instruction) {
                // Basic function
                case (byte)INST.PRINT_HELLO_WORLD:
                {
                    Debug.Log("Hello World");
                    break;
                }

                // Adds two values from the stack
                case (byte)INST.ADD:
                {
                    // NOTE: Since we're using a stack, variable order is reversed
                    // e.g. If we push A and B, we have to read back B then A
                    byte b = pop();
                    byte a = pop();
                    byte sum = (byte)(a + b);

                    push(sum);

                    Debug.Log($"Add (POP: {a}) + (POP: {b}) = (PUSH: {sum})");
                    break;
                }

                // Pushes a value to stack
                case (byte)INST.SET:
                {
                    byte value = bytecode[_curInstr++]; // Read next byte ( + skip next instruction since it's a value )
                    push(value);

                    Debug.Log($"Set (READ & PUSH: {value})");
                    break;
                }

                // Simple example to demostrate how we get a float from bytes
                case (byte)INST.PRINT_FLOAT:
                {
                    // Pop values back to forth
                    float value = System.BitConverter.ToSingle(new[] { pop(), pop(), pop(), pop() }, 0);

                    Debug.Log($"Print Float (POP 4x = {value})");
                    break;
                }

                // Simple example to demostrate how we get a float from bytes WITHOUT USING STACK
                case (byte)INST.PRINT_FLOAT_NO_STACK:
                {
                    // Read array back to forth
                    float value = System.BitConverter.ToSingle(new[] {
                                bytecode[_curInstr + 3],
                                bytecode[_curInstr + 2],
                                bytecode[_curInstr + 1],
                                bytecode[_curInstr]
                            }, 0);

                    _curInstr += 3; // Skip next 3 bytes ( 1 bytes is already skipped at start )

                    Debug.Log($"Print Float (READ 4x FOLLOWING BYTES = {value})");
                    break;
                }

                // Breaks loop and waits
                case (byte)INST.WAIT:
                {
                    int amount = pop();
                    wait = time + amount;
                    suspend = true;

                    Debug.Log($"Wait (POP: {amount})");
                    break;
                }

                // Breaks loop, clears stack and resets index
                case (byte)INST.REPEAT:
                {
                    _curInstr = 0;
                    suspend = true;
                    Array.Clear(_stack, 0, _stack.Length);

                    Debug.Log("Repeat");
                    break;
                }
            }
        }

        // Everything is done, no instructions left to execute
        if (_curInstr == bytecode.Length) {
            Debug.Log("Finished");
        }
    }

    /// <summary>
    /// Push a value to the stack
    /// </summary>
    /// <param name="value"></param>
    private void push (byte value)
    {
        Debug.Assert(_stackSize < MAX_STACK, "Stack overflow");
        _stack[_stackSize++] = value;
    }

    /// <summary>
    /// Pop a value from the stack
    /// </summary>
    /// <returns>Byte from array</returns>
    private byte pop ()
    {
        Debug.Assert(_stackSize > 0, "Stack is empty!");
        return _stack[--_stackSize];
    }
}
