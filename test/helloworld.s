.data

prompt: .asciiz "Qual o seu nome? "
str: .space 64


.text
# imprime string
li $v0, 4
la $a0, prompt
syscall

# le string
li $v0, 8
la $a0, str
li $a1, 64
syscall

# printa string lida
li $v0, 4
la $a0, str
syscall