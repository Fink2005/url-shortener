-- Create databases for each service
CREATE DATABASE auth_db;
CREATE DATABASE user_db;
CREATE DATABASE url_db;
CREATE DATABASE saga_db;

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE auth_db TO postgres;
GRANT ALL PRIVILEGES ON DATABASE user_db TO postgres;
GRANT ALL PRIVILEGES ON DATABASE url_db TO postgres;

