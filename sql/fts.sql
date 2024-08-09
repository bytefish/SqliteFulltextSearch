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
    data blob not null,
    uploaded_at timestamptz not null,
    indexed_at timestamptz null,
    last_edited_by integer not null,
    row_version integer default 1,
    valid_from text not null default (strftime('%Y-%m-%d %H:%M:%f', 'now')),
    valid_to text null default '9999-12-31',
    CONSTRAINT document_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES user(user_id), 

);

CREATE TABLE IF NOT EXISTS keyword (
    keyword_id integer PRIMARY KEY AUTOINCREMENT,
    name text not null,
    last_edited_by integer not null,
    row_version integer default 1,
    valid_from text not null default (strftime('%Y-%m-%d %H:%M:%f', 'now')),
    valid_to text null default '9999-12-31',
    CONSTRAINT keyword_pkey
        PRIMARY KEY (keyword_id),
    CONSTRAINT keyword_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES user(user_id),
    CHECK (length(name) <= 255)
);

CREATE TABLE IF NOT EXISTS suggestion (
    suggestion_id integer PRIMARY KEY AUTOINCREMENT,
    name text not null,
    last_edited_by integer not null,
    row_version integer default 1,
    valid_from text not null default (strftime('%Y-%m-%d %H:%M:%f', 'now')),
    valid_to text null default '9999-12-31',
    CONSTRAINT suggestion_pkey
        PRIMARY KEY (suggestion_id),
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
    title varchar(2000) not null,
    filename varchar(2000) not null,
    data blob not null,
    uploaded_at timestamptz not null,
    indexed_at timestamptz null,
    last_edited_by integer not null,
    row_version integer,
    valid_from text not null,
    valid_to text not null
);

CREATE TABLE IF NOT EXISTS keyword_history (
    keyword_id integer,
    name varchar(255) not null,
    last_edited_by integer not null,
    row_version integer,
    valid_from text not null,
    valid_to text null
);

CREATE TABLE IF NOT EXISTS suggestion_history (
    suggestion_id integer,
    name varchar(255) not null,
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
    AFTER INSERT ON document
BEGIN

    UPDATE 
        document
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER document_versioning_update_trigger
    AFTER UPDATE ON document
BEGIN

    INSERT INTO 
        document_history(document_id, title, filename, data, uploaded_at, last_edited_by, row_version, valid_from, valid_to)
    SELECT 
        document_id, title, filename, data, uploaded_at, last_edited_by, row_version, valid_from, NEW.valid_from
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

CREATE TRIGGER document_versioning_delete_trigger
    AFTER DELETE ON document
BEGIN
    INSERT INTO 
        document_history(document_id, title, filename, data, uploaded_at, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        OLD.document_id, OLD.title, OLD.filename, OLD.data, OLD.uploaded_at, OLD.last_edited_by, OLD.row_version, OLD.valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');
END;

-- History Triggers "keyword"
CREATE TRIGGER keyword_versioning_insert_trigger
    AFTER INSERT ON keyword
BEGIN

    UPDATE 
        keyword
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER keyword_versioning_update_trigger
    AFTER UPDATE ON keyword
BEGIN

    INSERT INTO 
        keyword_history(keyword_id, name, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        keyword_id, name, last_edited_by, row_version, valid_from, NEW.valid_to
    WHERE
        rowid = NEW.rowid;

    UPDATE 
        keyword
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER keyword_versioning_delete_trigger
    AFTER DELETE ON keyword
BEGIN

    INSERT INTO 
        keyword_history(keyword_id, name, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        OLD.keyword_id, OLD.name, OLD.last_edited_by, OLD.row_version, OLD.valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');

END;

-- History Triggers "suggestion"
CREATE TRIGGER suggestion_versioning_insert_trigger
    AFTER INSERT ON suggestion
BEGIN

    UPDATE 
        suggestion
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER suggestion_versioning_update_trigger
    AFTER UPDATE ON suggestion
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

CREATE TRIGGER suggestion_versioning_delete_trigger
    AFTER DELETE ON suggestion
BEGIN
    INSERT INTO 
        suggestion_history(suggestion_id, name, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        suggestion_id, name, last_edited_by, row_version, valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');
END;

-- History Triggers "document_keyword"
CREATE TRIGGER document_keyword_versioning_insert_trigger
    AFTER INSERT ON document_keyword
BEGIN

    UPDATE 
        document_keyword
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER document_keyword_versioning_update_trigger
    AFTER UPDATE ON document_keyword
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

CREATE TRIGGER document_keyword_versioning_delete_trigger
    AFTER DELETE ON document_keyword
BEGIN

    INSERT INTO 
        document_keyword_history(document_keyword_id, document_id, keyword_id, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        OLD.document_keyword_id, OLD.document_id, OLD.keyword_id, OLD.last_edited_by, OLD.row_version, OLD.valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');
        
END;

-- History Triggers "document_suggestion"
CREATE TRIGGER document_suggestion_versioning_insert_trigger
    AFTER INSERT ON document_suggestion
BEGIN

    UPDATE 
        document_suggestion
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;


END;

CREATE TRIGGER document_suggestion_versioning_update_trigger
    AFTER UPDATE ON document_suggestion
BEGIN

    INSERT INTO 
        document_suggestion_history(document_suggestion_id, document_id, keyword_id, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        document_suggestion_id, document_id, keyword_id, last_edited_by, row_version, valid_from, valid_to
    WHERE
        rowid = NEW.rowid;

    UPDATE 
        document_suggestion
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER document_suggestion_versioning_delete_trigger
    AFTER DELETE ON document_suggestion
BEGIN

    INSERT INTO 
        document_suggestion_history(document_suggestion_id, document_id, keyword_id, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        OLD.document_suggestion_id, OLD.document_id, OLD.keyword_id, OLD.last_edited_by, OLD.row_version, OLD.valid_from,  strftime('%Y-%m-%d %H:%M:%f', 'now');

END;

-- FTS5 Document Search Table
CREATE VIRTUAL TABLE fts_document 
    USING fts5(title, content);