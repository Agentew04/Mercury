ENTRY(__start)                        /* Sets the entry point to _start */

SECTIONS
{
    // abaixo da conflito com o text especificado em 0x0040 0000
    //. = 0x00400000;                  /* Start address of the program */

    .text 0x00400000: {
        *(.text)                     /* Place all .text sections here */
    }

    .data 0x10010000: {
        *(.data)                     /* Place all .data sections here */
    }

    // deixa automaticamente o bss e got ao final do data
    /*.bss : {
        *(.bss)                      // Place all .bss sections here 
    }

    .rodata : {
        *(.rodata)                   // Place read-only data here 
    }*/

    . = ALIGN(4K);                   /* Align to page size */
    _end = .;                        /* Mark the end of the program */
}
