       IDENTIFICATION DIVISION.
       PROGRAM-ID. ProcCall.
      
       DATA DIVISION.
       WORKING-STORAGE SECTION.
       01 PROGRAM_NAME      pic X(08) value 'PGM00001'.
       01 PARAMETER_1       pic X(08).
       01 PARAMETER_2       pic X(08).
      
       PROCEDURE DIVISION.
        
       call 'zcallpgm' using PROGRAM_NAME
                                PARAMETER_1
                                PARAMETER_2.
      
       call 'ZCALLPGM' using by content PROGRAM_NAME
                                PARAMETER_1.
       
       call 'zCaLlpgm' using by reference PROGRAM_NAME.
       
            call 'zCaLlpgm' using by value PROGRAM_NAME.
      
       END PROGRAM ProcCall.