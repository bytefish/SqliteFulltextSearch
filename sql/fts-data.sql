DO $$

BEGIN

-- Initial Data
INSERT INTO user(user_id, email, preferred_name, last_edited_by) 
    VALUES 
        (1, 'philipp@bytefish.de', 'Data Conversion User', 1);

END;
$$ LANGUAGE plpgsql;
