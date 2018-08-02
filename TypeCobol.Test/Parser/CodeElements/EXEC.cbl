EXEC SQL
  USE TEST
END-EXEC
EXEC SQL
  SELECT 'toto' from blabla
  -- testcomment
  /* comment2 */
  SELECT "identity","order"   
	FROM "select"  
	ORDER BY "order";
  FETCH EMPTBL INTO 
           :ENO,:LNAME,:FNAME,:STREET,:CITY, 
           :ST,:ZIP,:DEPT,:PAYRATE, 
           :COM :COM-NULL-IND
   SELECT DISTINCT projno, empprojact.empno,
                   lastname||", "||firstnme ,salary
           from corpdata/empprojact, corpdata/employee
           where empprojact.empno =employee.empno and
                 comm >= :commission
           order by projno, empno

END-EXEC
EXEC SQL
  UPDATE TIDCA01
  SET x = :y
    WHERE a = :b
      AND c = :d
      AND e = :f
      AND g = :h
END-EXEC
EXEC ramblings
herp derp derp derrrpp herrp herpherp herp derp
AAAAAAAAAAAAAAAAAAAAAAAA
MY HANDS ARE TYPING WORDS
HAAAAAAAAAAAAANNDS
%$г*ииии#@з
herp derp derp heeeeeeeeeeeeerp "derp" derp
END-EXEC

EXEC witespace

    
			
      
END-EXEC

EXEC cobol
IF condition THEN
  CONTINUE
ELSE
  CONTINUE
END-IF.
END-EXEC

EXEC empty END-EXEC