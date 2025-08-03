---
title: System Calls
---

# System Calls

This document lists all the available system calls and their usage for the **Linux 1.0** operating system targeting the **MIPS** architecture.


| Name | Number | Description |
|------|--------|-------------|
| Print Integer | 1 | Prints the integer value in `$a0` to standard output. |
| Print Float 32-bit | 2 | Prints the floating-point value in `$f12` to standard output. |
| Print Float 64-bit | 3 | Prints the floating-point value in `$f12` to standard output. |
| Print String | 4 | Prints a null-terminated string whose base address is in `$a0`. |
| Read Integer | 5 | Reads an integer from standard input and stores it in `$v0`. |
| Read Float 32-bit | 6 | Reads a floating-point number from standard input and stores it in `$f0`. |
| Read Float 64-bit | 7 | Reads a floating-point number from standard input and stores it in `$f0`. |
| Read String | 8 | Reads a string from standard input and stores it in the base address at `$a0`. `$a1` contains the buffer size (reads `$a1 - 1` characters). |
| SBRK | 9 | Allocates dynamic memory. `$a0` contains the number of bytes to allocate. The base address of the allocated block is returned in `$v0`. |
| Exit | 10 | Terminates the program. |
| Print Character | 11 | Prints the character in `$a0` to standard output. |
| Read Character | 12 | Reads a character from standard input and stores it in `$v0`. |
| Open File | 13 | Opens a file whose name is at the base address in `$a0`. Mode is in `$a1` (0: read, 1: write, 2: read/write). Returns file descriptor in `$v0`. Negative value indicates error. |
| Read from File | 14 | Reads bytes from an open file. `$a0`: file descriptor. `$a1`: buffer address. `$a2`: number of bytes to read. Returns bytes read in `$v0`, negative for error or EOF. |
| Write to File | 15 | Writes bytes to an open file. `$a0`: file descriptor. `$a1`: buffer address. `$a2`: number of bytes to write. Returns bytes written in `$v0`, negative on error. |
| Close File | 16 | Closes an open file. `$a0`: file descriptor to close. |
| Exit with Code | 17 | Terminates program with `$a0` as exit code. |
| -- | -- | Calls below 17 are also compatible with the SPIM simulator. |
| System Time | 30 | Returns current system time. `$a0`: lower 32 bits, `$a1`: upper 32 bits. |
| Play Async Sound | 31 | Plays a sound and returns immediately. `$a0`: pitch (0–127), `$a1`: duration in ms, `$a2`: instrument (0–127), `$a3`: volume (0–127). |
| Sleep | 32 | Waits for `$a0` milliseconds. |
| Play Sync Sound | 33 | Plays a sound and returns after it's finished. Same params as `31`. |
| Print Hex Integer | 34 | Prints the integer in `$a0` as a hexadecimal value. |
| Print Binary Integer | 35 | Prints the integer in `$a0` as a 32-bit binary value, padded with leading 0s. |
| Print Unsigned Integer | 36 | Prints the unsigned integer in `$a0`. |
| Set RNG Seed | 40 | Sets the seed for the RNG. `$a0`: RNG id, `$a1`: seed. |
| Generate Random Number | 41 | Generates a random number. `$a0`: RNG id. Result returned in `$a0`. |
| Generate Random Number in Range | 42 | Generates a random number in range `[0, N]`. `$a0`: RNG id, `$a1`: upper bound. Result in `$a0`. |
| Generate Random Float 32-bit | 43 | Generates a 32-bit float in `[0,1]`. `$a0`: RNG id. Result in `$f0`. |
| Generate Random Float 64-bit | 44 | Generates a 64-bit float in `[0,1]`. `$a0`: RNG id. Result in `$f0`. |
| Print Boolean | 45 | Prints a boolean value from `$a0` to standard output. |
| Read Boolean | 46 | Reads a boolean value from standard input and stores it in `$v0`. |