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
