-- Tables
CREATE TABLE IF NOT EXISTS migration (
    migration_id integer PRIMARY KEY AUTOINCREMENT,
    version text not null
        CHECK (length(version) <= 255),
    description text not null
        CHECK (length(description) <= 2000),
    last_edited_by integer not null,
    row_version integer default 1,
    valid_from text not null default (strftime('%Y-%m-%d %H:%M:%f', 'now')),
    valid_to text null default '9999-12-31',
    CONSTRAINT user_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES user(user_id)
);

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
CREATE TABLE IF NOT EXISTS migration_history (
    migration_id integer,
    version text not null, 
    description text not null,
    last_edited_by integer not null,
    row_version integer not null,
    valid_from text not null,
    valid_to text
);

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

-- History Triggers "migration_history"
CREATE TRIGGER IF NOT EXISTS migration_insert_trigger
    AFTER INSERT ON migration FOR EACH ROW
BEGIN

    UPDATE 
        migration_history
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;
   
END;

CREATE TRIGGER IF NOT EXISTS migration_update_trigger
    AFTER UPDATE ON migration FOR EACH ROW
BEGIN

    INSERT INTO 
        migration_history(migration_id, version, description, last_edited_by, row_version, valid_from, valid_to)
    SELECT 
        migration_id, version, description, last_edited_by, row_version, valid_from, NEW.valid_from
    FROM 
        migration
    WHERE
        rowid = NEW.rowid;
        
    UPDATE 
        migration
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;
    
END;

CREATE TRIGGER IF NOT EXISTS migration_delete_trigger
    AFTER DELETE ON user FOR EACH ROW
BEGIN

    INSERT INTO 
        migration_history(migration_id, version, description, last_edited_by, row_version, valid_from, valid_to)
    SELECT 
        migration_id, version, description, last_edited_by, row_version, valid_from, NEW.valid_from
    FROM 
        migration
    WHERE
        rowid = NEW.rowid;
        
END;

-- History Triggers "user"
CREATE TRIGGER IF NOT EXISTS user_insert_trigger
    AFTER INSERT ON user FOR EACH ROW
BEGIN

    UPDATE 
        user
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;
   
END;

CREATE TRIGGER IF NOT EXISTS user_update_trigger
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

CREATE TRIGGER IF NOT EXISTS user_delete_trigger
    AFTER DELETE ON user FOR EACH ROW
BEGIN

    INSERT INTO 
        user_history(user_id, email, preferred_name, last_edited_by, row_version, valid_from, valid_to)
    SELECT 
        OLD.user_id, OLD.email, OLD.preferred_name, OLD.last_edited_by, OLD.row_version, OLD.valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');
        
END;

-- History Triggers "document"
CREATE TRIGGER IF NOT EXISTS document_insert_trigger
    AFTER INSERT ON document FOR EACH ROW
BEGIN

    UPDATE 
        document
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER IF NOT EXISTS document_update_trigger
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

CREATE TRIGGER IF NOT EXISTS document_delete_trigger
    AFTER DELETE ON document FOR EACH ROW
BEGIN
    INSERT INTO 
        document_history(document_id, title, filename, data, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        OLD.document_id, OLD.title, OLD.filename, OLD.data, OLD.last_edited_by, OLD.row_version, OLD.valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');
END;

-- History Triggers "keyword"
CREATE TRIGGER IF NOT EXISTS keyword_insert_trigger
    AFTER INSERT ON keyword FOR EACH ROW
BEGIN

    UPDATE 
        document
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;
        
END;


CREATE TRIGGER IF NOT EXISTS keyword_update_trigger
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

CREATE TRIGGER IF NOT EXISTS keyword_delete_trigger
    AFTER DELETE ON keyword FOR EACH ROW
BEGIN

    INSERT INTO 
        keyword_history(keyword_id, name, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        OLD.keyword_id, OLD.name, OLD.last_edited_by, OLD.row_version, OLD.valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');

END;

-- History Triggers "suggestion"
CREATE TRIGGER IF NOT EXISTS suggestion_insert_trigger
    AFTER INSERT ON suggestion FOR EACH ROW
BEGIN

    UPDATE 
        suggestion
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER IF NOT EXISTS suggestion_update_trigger
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

CREATE TRIGGER IF NOT EXISTS suggestion_delete_trigger
    AFTER DELETE ON suggestion FOR EACH ROW
BEGIN
    INSERT INTO 
        suggestion_history(suggestion_id, name, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        suggestion_id, name, last_edited_by, row_version, valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');
END;

-- History Triggers "document_keyword"
CREATE TRIGGER IF NOT EXISTS document_keyword_insert_trigger
    AFTER INSERT ON document_keyword FOR EACH ROW
BEGIN

    UPDATE 
        document_keyword
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER IF NOT EXISTS document_keyword_update_trigger
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

CREATE TRIGGER IF NOT EXISTS document_keyword_delete_trigger
    AFTER DELETE ON document_keyword FOR EACH ROW
BEGIN

    INSERT INTO 
        document_keyword_history(document_keyword_id, document_id, keyword_id, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        OLD.document_keyword_id, OLD.document_id, OLD.keyword_id, OLD.last_edited_by, OLD.row_version, OLD.valid_from, strftime('%Y-%m-%d %H:%M:%f', 'now');
        
END;

-- History Triggers "document_suggestion"
CREATE TRIGGER IF NOT EXISTS document_suggestion_insert_trigger
    AFTER INSERT ON document_suggestion FOR EACH ROW
BEGIN

    UPDATE 
        document_suggestion
    SET 
        row_version = row_version + 1
    WHERE 
        rowid = NEW.rowid;

END;

CREATE TRIGGER IF NOT EXISTS document_suggestion_update_trigger
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

CREATE TRIGGER IF NOT EXISTS document_suggestion_delete_trigger
    AFTER DELETE ON document_suggestion FOR EACH ROW
BEGIN

    INSERT INTO 
        document_suggestion_history(document_suggestion_id, document_id, suggestion_id, last_edited_by, row_version, valid_from, valid_to)
    SELECT
        OLD.document_suggestion_id, OLD.document_id, OLD.suggestion_id, OLD.last_edited_by, OLD.row_version, OLD.valid_from,  strftime('%Y-%m-%d %H:%M:%f', 'now');

END;

-- FTS5 Document Search Table
CREATE VIRTUAL TABLE IF NOT EXISTS fts_document 
    USING fts5(title, content);

-- FTS5 Suggestions Search Table
CREATE VIRTUAL TABLE IF NOT EXISTS fts_suggestion 
    USING fts5(name);

-- Insert Fixed data
INSERT OR IGNORE INTO user(user_id, email, preferred_name, last_edited_by) 
SELECT 1, 'philipp@bytefish.de', 'Data Conversion User', 1
WHERE NOT EXISTS(SELECT 1 FROM user where user_id = 1);        

-- Set the Database Version for an initial create
INSERT INTO 
    migration(migration_id, version, description, last_edited_by) 
SELECT 1, '1.0',  'Initial Database Creation (Version 1.0)', 1
WHERE NOT EXISTS(SELECT 1 FROM migration);
