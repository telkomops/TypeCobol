﻿       IDENTIFICATION DIVISION.
       PROGRAM-ID.   Test-UNSAFE.
       
       DATA DIVISION.
       WORKING-STORAGE SECTION.
       01 SmallGroup TYPEDEF strict.
         05 x PIC 9(04).
         05 y PIC 9(04).
       01 ToughGroup TYPEDEF strict.
         05 x PIC 9(04).
         05 y PIC 9(04).
       01 Small TYPEDEF strict  PIC 9(04).

       01 identifier-1 TYPE ToughGroup.
       01 identifier-2 TYPE SmallGroup.
       01 identifier-3 TYPE Small.
       01 identifier-4 PIC 9(04).
       01 myunsafeid PIC 9(04).
       01 myunsafeTxt PIC X(14).

       PROCEDURE DIVISION.
      * OK: receiver is strongly-typed, with UNSAFE sender can be of 
      * a different TYPE
           MOVE UNSAFE CORRESPONDING identifier-2 TO identifier-1
           MOVE UNSAFE identifier-2 TO identifier-1
           MOVE unsafe identifier-3 TO identifier-1
      * OK: receiver is strongly-typed, 
      * with UNSAFE sender can have no TYPE
           MOVE UNSAFE identifier-4 TO identifier-1
           MOVE UNSAFE '1337'       TO identifier-1
      * WARN: receiver is weakly-typed, UNSAFE is useless
           MOVE UNSAFE CORRESPONDING identifier-1 TO identifier-2
           MOVE unsafe identifier-1 TO identifier-2
           MOVE UNSAFE identifier-4 TO identifier-3
           MOVE UNSAFE '1337'       TO identifier-3
           MOVE UNSAFE identifier-3 TO identifier-4
           MOVE UNSAFE '1337'       TO identifier-4
      * OK: only exact matches to unsafe are deleted in output code
           MOVE UNSAFE myunsafeid   TO identifier-4
           MOVE UNSAFE identifier-4 TO myunsafeid
           MOVE        myunsafeid   TO identifier-4
           MOVE        myUNSAFEid   TO identifier-4
           MOVE unsaFe myUNSAFEid   TO identifier-4
           MOVE uNsaFe myUNSAFEid   TO identifier-4
           MOVE uNsaFe 'unsafe'     TO myunsafeTxt
           MOVE uNsaFe ' unsafe '   TO myunsafeTxt
           MOVE uNsaFe " unsafe "   TO myunsafeTxt
           MOVE uNsaFe " 'unsafe' "          TO myunsafeTxt
           MOVE uNsaFe " 'unsafe' unsafe "   TO myunsafeTxt
      * Test a long line
           MOVE UNSAFE myunsafeid   TO                     identifier-4
           .

       END PROGRAM Test-UNSAFE.