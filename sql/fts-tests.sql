CREATE OR REPLACE PROCEDURE fts.cleanup_tests()
AS $cleanup_tests_func$
BEGIN

	-- Delete all non-fixed data
	DELETE FROM fts.document_keyword;
	DELETE FROM fts.document_suggestion;
	DELETE FROM fts.keyword;
	DELETE FROM fts.suggestion;
	DELETE FROM fts.document;	
	DELETE FROM fts.outbox_event;	
	
    DELETE FROM fts.user WHERE user_id != 1;
	
	-- Delete historic data
	DELETE FROM fts.document_keyword_history;
	DELETE FROM fts.document_suggestion_history;
	DELETE FROM fts.keyword_history;
	DELETE FROM fts.suggestion_history;
	DELETE FROM fts.document_history;
	DELETE FROM fts.outbox_event_history;	
	
    DELETE FROM fts.user_history WHERE user_id != 1;
	
    
END; $cleanup_tests_func$ 
LANGUAGE plpgsql;