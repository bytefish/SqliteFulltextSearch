-- Tables
CREATE TABLE IF NOT EXISTS user (
    user_id integer PRIMARY KEY AUTOINCREMENT,
    email text not null
        CHECK (length(email) <= 1000),
    preferred_name text not null
        CHECK (length(preferred_name) <= 1000),
    last_edited_by integer not null,
    row_version integer default 1,
    valid_from text not null default (strftime('%Y-%m-%d %H:%M:%f', 'now')),
    valid_to text null default '9999-12-31',
    CONSTRAINT user_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES user(user_id)
);

CREATE TABLE IF NOT EXISTS document (
    document_id integer PRIMARY KEY AUTOINCREMENT,
    title text not null
        CHECK (length(title) <= 1000),
    filename text not null
        CHECK (length(filename) <= 1000),
    data blob null,
    last_edited_by integer not null,
    row_version integer default 1,
    valid_from text not null default (strftime('%Y-%m-%d %H:%M:%f', 'now')),
    valid_to text null default '9999-12-31',
    CONSTRAINT document_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES user(user_id)

);

CREATE TABLE IF NOT EXISTS keyword (
    keyword_id integer PRIMARY KEY AUTOINCREMENT,
    name text not null
        CHECK (length(name) <= 255),
    last_edited_by integer not null,
    row_version integer default 1,
    valid_from text not null default (strftime('%Y-%m-%d %H:%M:%f', 'now')),
    valid_to text null default '9999-12-31',
    CONSTRAINT keyword_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES user(user_id)
);

CREATE TABLE IF NOT EXISTS suggestion (
    suggestion_id integer PRIMARY KEY AUTOINCREMENT,
    name text not null
        CHECK (length(name) <= 255),
    last_edited_by integer not null,
    row_version integer default 1,
    valid_from text not null default (strftime('%Y-%m-%d %H:%M:%f', 'now')),
    valid_to text null default '9999-12-31',
    CONSTRAINT suggestion_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES user(user_id),
    CHECK (length(name) <= 255) 
);

CREATE TABLE IF NOT EXISTS document_keyword (
    document_keyword_id integer PRIMARY KEY AUTOINCREMENT,
    document_id int not null,
    keyword_id int not null,
    last_edited_by integer not null,
    row_version integer default 1,
    valid_from text not null default (strftime('%Y-%m-%d %H:%M:%f', 'now')),
    valid_to text null default '9999-12-31',
    CONSTRAINT document_keyword_document_id_fkey 
        FOREIGN KEY (document_id)
        REFERENCES document(document_id),
    CONSTRAINT document_keyword_keyword_id_fkey 
        FOREIGN KEY (keyword_id)
        REFERENCES keyword(keyword_id),
    CONSTRAINT document_keyword_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES user(user_id)
);

CREATE TABLE IF NOT EXISTS document_suggestion (
    document_suggestion_id integer PRIMARY KEY AUTOINCREMENT,
    document_id int not null,
    suggestion_id int not null,
    last_edited_by integer not null,
    row_version integer default 1,
    valid_from text not null default (strftime('%Y-%m-%d %H:%M:%f', 'now')),
    valid_to text null default '9999-12-31',
    CONSTRAINT document_suggestion_document_id_fkey 
        FOREIGN KEY (document_id)
        REFERENCES document(document_id),
    CONSTRAINT document_suggestion_suggestion_id_fkey 
        FOREIGN KEY (suggestion_id)
        REFERENCES suggestion(suggestion_id),
    CONSTRAINT document_suggestion_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES user(user_id)
);

-- Indexes
CREATE UNIQUE INDEX IF NOT EXISTS user_email_key 
    ON user(email);

CREATE UNIQUE INDEX IF NOT EXISTS suggestion_name_key 
    ON suggestion(name);

CREATE UNIQUE INDEX IF NOT EXISTS keyword_name_key 
    ON keyword(name);

CREATE UNIQUE INDEX IF NOT EXISTS document_suggestion_document_id_suggestion_id_key 
    ON document_suggestion(document_id, suggestion_id);

CREATE UNIQUE INDEX IF NOT EXISTS document_keyword_document_id_keyword_id_key 
    ON document_keyword(document_id, keyword_id);

-- History Tables
CREATE TABLE IF NOT EXISTS user_history (
    user_id integer,
    email text not null,
    preferred_name text not null,
    last_edited_by integer not null,
    row_version integer,
    valid_from text not null,
    valid_to text not null
);

CREATE TABLE IF NOT EXISTS document_history (
    document_id integer,
    title text not null,
    filename text not null,
    data blob null,
    last_edited_by integer not null,
    row_version integer,
    valid_from text not null,
    valid_to text not null
);

CREATE TABLE IF NOT EXISTS keyword_history (
    keyword_id integer,
    name text not null,
    last_edited_by integer not null,
    row_version integer,
    valid_from text not null,
    valid_to text null
);

CREATE TABLE IF NOT EXISTS suggestion_history (
    suggestion_id integer,
    name text not null,
    last_edited_by integer not null,
    row_version integer,
    valid_from text not null,
    valid_to text not null
);

CREATE TABLE IF NOT EXISTS document_keyword_history (
    document_keyword_id integer,
    document_id int not null,
    keyword_id int not null,
    last_edited_by integer not null,
    row_version integer,
    valid_from text not null,
    valid_to text not null
);

CREATE TABLE IF NOT EXISTS document_suggestion_history (
    document_suggestion_id integer,
    document_id int not null,
    suggestion_id int not null,
    last_edited_by integer not null,
    row_version integer,
    valid_from text not null,
    valid_to text not null
);

-- History Triggers "user"
CREATE TRIGGER user_insert_trigger
    AFTER INSERT ON user FOR EACH ROW
BEGIN

    UPDATE 
        user
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;
   
END;

CREATE TRIGGER user_update_trigger
    AFTER UPDATE ON user FOR EACH ROW
BEGIN

    INSERT INTO 
        user_history(user_id, email, preferred_name, last_edited_by, row_version, valid_from, valid_to)
    SELECT 
        user_id, email, preferred_name, last_edited_by, row_version, valid_from, NEW.valid_from
    FROM 
        user
    WHERE
        rowid = NEW.rowid;
        
    UPDATE 
        user
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;
    
END;

CREATE TRIGGER user_delete_trigger
    AFTER DELETE ON user FOR EACH ROW
BEGIN

    INSERT INTO 
        user_history(user_id, email, preferred_name, last_edited_by, row_version, valid_from, valid_to)
    SELECT 
        OLD.user_id, OLD.email, OLD.preferred_name, OLD.last_edited_by, OLD.row_version, OLD.valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');
        
END;

-- History Triggers "document"
CREATE TRIGGER document_insert_trigger
    AFTER INSERT ON document FOR EACH ROW
BEGIN

    UPDATE 
        document
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER document_update_trigger
    AFTER UPDATE ON document FOR EACH ROW
BEGIN

    INSERT INTO 
        document_history(document_id, title, filename, data, last_edited_by, row_version, valid_from, valid_to)
    SELECT 
        document_id, title, filename, data, last_edited_by, row_version, valid_from, NEW.valid_from
    FROM 
        document
    WHERE 
        rowid = NEW.rowid;

    UPDATE 
        document
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER document_delete_trigger
    AFTER DELETE ON document FOR EACH ROW
BEGIN
    INSERT INTO 
        document_history(document_id, title, filename, data, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        OLD.document_id, OLD.title, OLD.filename, OLD.data, OLD.last_edited_by, OLD.row_version, OLD.valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');
END;

-- History Triggers "keyword"
CREATE TRIGGER keyword_insert_trigger
    AFTER INSERT ON keyword FOR EACH ROW
BEGIN

    UPDATE 
        document
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;
        
END;


CREATE TRIGGER keyword_update_trigger
    AFTER UPDATE ON keyword FOR EACH ROW
BEGIN

    INSERT INTO 
        keyword_history(keyword_id, name, last_edited_by, row_version, valid_from, valid_to)
    SELECT 
        keyword_id, name, last_edited_by, row_version, valid_from, NEW.valid_to
    FROM
        keyword
    WHERE
        rowid = NEW.rowid;

    
    UPDATE 
        keyword
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER keyword_delete_trigger
    AFTER DELETE ON keyword FOR EACH ROW
BEGIN

    INSERT INTO 
        keyword_history(keyword_id, name, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        OLD.keyword_id, OLD.name, OLD.last_edited_by, OLD.row_version, OLD.valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');

END;

-- History Triggers "suggestion"
CREATE TRIGGER suggestion_insert_trigger
    AFTER INSERT ON suggestion FOR EACH ROW
BEGIN

    UPDATE 
        suggestion
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER suggestion_update_trigger
    AFTER UPDATE ON suggestion FOR EACH ROW
BEGIN

    INSERT INTO 
        suggestion_history(suggestion_id, name, last_edited_by, row_version, valid_from, valid_to)
    SELECT 
        suggestion_id, name, last_edited_by, row_version, valid_from, NEW.valid_to
    FROM
        suggestion
    WHERE
        rowid = NEW.rowid;

    UPDATE 
        suggestion
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER suggestion_delete_trigger
    AFTER DELETE ON suggestion FOR EACH ROW
BEGIN
    INSERT INTO 
        suggestion_history(suggestion_id, name, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        suggestion_id, name, last_edited_by, row_version, valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');
END;

-- History Triggers "document_keyword"
CREATE TRIGGER document_keyword_insert_trigger
    AFTER INSERT ON document_keyword FOR EACH ROW
BEGIN

    UPDATE 
        document_keyword
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER document_keyword_update_trigger
    AFTER UPDATE ON document_keyword FOR EACH ROW
BEGIN

    INSERT INTO 
        document_keyword_history(document_keyword_id, document_id, keyword_id, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        document_keyword_id, document_id, keyword_id, last_edited_by, row_version, valid_from, NEW.valid_to
    FROM 
        document_keyword
    WHERE
        rowid = NEW.rowid;

    UPDATE 
        document_keyword
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER document_keyword_delete_trigger
    AFTER DELETE ON document_keyword FOR EACH ROW
BEGIN

    INSERT INTO 
        document_keyword_history(document_keyword_id, document_id, keyword_id, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        OLD.document_keyword_id, OLD.document_id, OLD.keyword_id, OLD.last_edited_by, OLD.row_version, OLD.valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');
        
END;

-- History Triggers "document_suggestion"
CREATE TRIGGER document_suggestion_insert_trigger
    AFTER INSERT ON document_suggestion FOR EACH ROW
BEGIN

    UPDATE 
        document_suggestion
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER document_suggestion_update_trigger
    AFTER UPDATE ON document_suggestion FOR EACH ROW
BEGIN

    INSERT INTO 
        document_suggestion_history(document_suggestion_id, document_id, suggestion_id, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        document_suggestion_id, document_id, suggestion_id, last_edited_by, row_version, valid_from, NEW.valid_to
    FROM 
        document_suggestion
    WHERE
        rowid = NEW.rowid;

    UPDATE 
        document_suggestion
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER document_suggestion_delete_trigger
    AFTER DELETE ON document_suggestion FOR EACH ROW
BEGIN

    INSERT INTO 
        document_suggestion_history(document_suggestion_id, document_id, suggestion_id, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        OLD.document_suggestion_id, OLD.document_id, OLD.suggestion_id, OLD.last_edited_by, OLD.row_version, OLD.valid_from,  strftime('%Y-%m-%d %H:%M:%f', 'now');

END;

-- FTS5 Document Search Table
CREATE VIRTUAL TABLE fts_document 
    USING fts5(title, content);

-- FTS5 Suggestions Search Table
CREATE VIRTUAL TABLE fts_suggestion 
    USING fts5(name);

-- Sample Data
INSERT INTO user(user_id, email, preferred_name, last_edited_by) 
    VALUES 
        (1, 'philipp@bytefish.de', 'Data Conversion User', 1);

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
        'highlight', suggestions_cte.match_suggestion,
        'row_version', suggestion.row_version,
        'last_edited_by', suggestion.last_edited_by,
        'valid_from', suggestion.valid_from,
        'valid_to', suggestion.valid_to
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
        f.fts_document MATCH '{title content}: and' 
    ORDER BY f.rank
) 
SELECT json_group_array(
    json_object(
        'document_id', document.document_id,
        'filename', document.filename,
        'row_version', document.row_version,
        'last_edited_by', document.last_edited_by,
        'valid_from', document.valid_from,
        'valid_to', document.valid_to,
        'keywords', (
            SELECT json_group_array(json_object(
                'keyword_id', k.keyword_id, 
                'name', k.name, 
                'row_version', k.row_version, 
                'last_edited_by', k.last_edited_by, 
                'valid_from', k.valid_from, 
                'valid_to', k.valid_to))
            FROM document_keyword dk
                INNER JOIN keyword k on dk.keyword_id = k.keyword_id
            WHERE 
                dk.document_id = documents_cte.document_id
         ),
         'suggestions', (
            SELECT json_group_array(json_object(
                'suggestion_id', s.suggestion_id, 
                'name', s.name, 
                'row_version', s.row_version, 
                'last_edited_by', s.last_edited_by, 
                'valid_from', s.valid_from, 
                'valid_to', s.valid_to))
            FROM document_suggestion ds
                INNER JOIN suggestion s on ds.suggestion_id = s.suggestion_id
            WHERE 
                ds.document_id = documents_cte.document_id
         ),
         'matches', json_object(
            'title', documents_cte.match_title, 
            'content', documents_cte.match_content)
    )
)
FROM documents_cte
    INNER JOIN document document ON documents_cte.document_id = document.document_id; 
