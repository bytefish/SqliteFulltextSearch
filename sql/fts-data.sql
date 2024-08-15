-- Document "Machine Learning with OpenCV"
INSERT INTO 
    document(document_id, title, filename, last_edited_by)
VALUES 
    (1, 'Machine Learning with OpenCV', 'machinelearning.pdf', 1);
   
INSERT INTO 
    keyword(keyword_id, name, last_edited_by)
VALUES 
    (1, 'Machine Learning', 1);
    
INSERT INTO 
    keyword(keyword_id, name, last_edited_by)
VALUES 
    (2, 'OpenCV', 1);
     
INSERT INTO 
    document_keyword(document_keyword_id, document_id, keyword_id, last_edited_by)
VALUES 
    (1, 1, 1, 1);
    
INSERT INTO 
    document_keyword(document_keyword_id, document_id, keyword_id, last_edited_by)
VALUES 
    (2, 1, 2, 1);
    
INSERT INTO 
    suggestion(suggestion_id, name, last_edited_by)
VALUES 
    (1, 'Machine Learning with OpenCV', 1);
    
INSERT INTO 
    document_suggestion(document_suggestion_id, document_id, suggestion_id, last_edited_by)
VALUES 
    (1, 1, 1, 1);

-- Document Face Recognition with GNU Octave/MATLAB
INSERT INTO 
    document(document_id, title, filename, last_edited_by)
VALUES 
    (2, 'Face Recognition with GNU Octave/MATLAB', 'facerecognition.pdf', 1);
 
INSERT INTO 
    keyword(keyword_id, name, last_edited_by)
VALUES 
    (3, 'MATLAB', 1);
    
INSERT INTO 
    keyword(keyword_id, name, last_edited_by)
VALUES 
    (4, 'Face Recognition', 1);
     
INSERT INTO 
    document_keyword(document_keyword_id, document_id, keyword_id, last_edited_by)
VALUES 
    (3, 2, 3, 1);
    
INSERT INTO 
    document_keyword(document_keyword_id, document_id, keyword_id, last_edited_by)
VALUES 
    (4, 2, 4, 1);
    
INSERT INTO 
    suggestion(suggestion_id, name, last_edited_by)
VALUES 
    (2, 'Face Recognition with GNU Octave/MATLAB', 1);
    
INSERT INTO 
    suggestion(suggestion_id, name, last_edited_by)
VALUES 
    (3, 'GNU Octave/MATLAB', 1);
    
INSERT INTO 
    suggestion(suggestion_id, name, last_edited_by)
VALUES 
    (4, 'Face Recognition', 1);
    
INSERT INTO 
    suggestion(suggestion_id, name, last_edited_by)
VALUES 
    (5, 'Computer Vision', 1);
    
INSERT INTO 
    document_suggestion(document_suggestion_id, document_id, suggestion_id, last_edited_by)
VALUES 
    (2, 2, 2, 1);

INSERT INTO 
    document_suggestion(document_suggestion_id, document_id, suggestion_id, last_edited_by)
VALUES 
    (3, 2, 3, 1);

INSERT INTO 
    document_suggestion(document_suggestion_id, document_id, suggestion_id, last_edited_by)
VALUES 
    (4, 2, 4, 1);

INSERT INTO 
    document_suggestion(document_suggestion_id, document_id, suggestion_id, last_edited_by)
VALUES 
    (5, 2, 5, 1);

-- Insert FTS Suggestion Data    
INSERT INTO 
    fts_suggestion(rowid, name)
VALUES
     (1, 'Machine Learning with OpenCV')
    ,(2, 'Face Recognition with GNU Octave/MATLAB')
    ,(3, 'GNU Octave/MATLAB')
    ,(4, 'Face Recognition')
    ,(5, 'Computer Vision');

-- Insert FTS Document Data
INSERT INTO 
    fts_document(rowid, title, content)
VALUES
    (1, 'Machine Learning with OpenCV', concat('This document covers the Machine Learning API of the OpenCV2 C++ API.'
            ,' It helps you with setting up your system, gives a brief introduction into Support Vector Machines'
            ,' and Neural Networks and shows how it’s implemented with OpenCV.')),
    (2, 'Face Recognition with GNU Octave/MATLAB', concat('In this document I’ll show you how to implement the Eigenfaces [13] and Fisherfaces [3] method'
            ,' with GNU Octave/MATLAB , so you’ll understand the basics of Face Recognition. All concepts'
            , ' are explained in detail, but a basic knowledge of GNU Octave/MATLAB is assumed.'));

-- Query for all Suggestions including Machine
WITH suggestions_cte AS 
(
    SELECT s.rowid suggestion_id, 
        highlight(s.fts_suggestion, 0, 'match→', '←match') match_suggestion
    FROM 
        fts_suggestion s
    WHERE 
        s.fts_suggestion MATCH '{suggestion}: Mach*' 
    ORDER BY s.rank
) 
SELECT json_group_array(
    json_object(
        'suggestion_id', suggestion.suggestion_id,
        'name', suggestion.name,
        'highlight', suggestions_cte.match_suggestion
    )
)
FROM suggestions_cte
    INNER JOIN suggestion suggestion ON suggestions_cte.suggestion_id = suggestion.suggestion_id; 

-- Query for all documents matching "OpenCV"
WITH documents_cte AS 
(
    SELECT f.rowid document_id, 
        snippet(f.fts_document, 0, 'match→', '←match', '', 32) match_title, 
        snippet(f.fts_document, 1, 'match→', '←match', '', 32) match_content
    FROM 
        fts_document f
    WHERE 
        f.fts_document MATCH '{title content}: Ma*' 
    ORDER BY f.rank
) 
SELECT json_group_array(
    json_object(
        'document_id', document.document_id,
        'title', document.title,
        'filename', document.filename,
        'match_title', documents_cte.match_title, 
        'match_content', documents_cte.match_content,
        'keywords', (
            SELECT json_group_array(json_object(
                'keyword_id', k.keyword_id, 
                'name', k.name))
            FROM document_keyword dk
                INNER JOIN keyword k on dk.keyword_id = k.keyword_id
            WHERE 
                dk.document_id = documents_cte.document_id
         ),
         'suggestions', (
            SELECT json_group_array(json_object(
                'suggestion_id', s.suggestion_id, 
                'name', s.name))
            FROM document_suggestion ds
                INNER JOIN suggestion s on ds.suggestion_id = s.suggestion_id
            WHERE 
                ds.document_id = documents_cte.document_id
         )
    )
)
FROM documents_cte
    INNER JOIN document document ON documents_cte.document_id = document.document_id; 
