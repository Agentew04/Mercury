ENTRY(__start)                        /* Sets the entry point to _start */

SECTIONS
{
    .text 0x00400000: {
        *(.text)                     /* Place all .text sections here */
    }

    .data 0x10010000: {
        *(.data)                     /* Place all .data sections here */
    }

    . = ALIGN(4K);                   /* Align to page size */
    __end = .;                        /* Mark the end of the program */

    metadata 0 : 
    {
        KEEP(*(metadata))
    }
}
