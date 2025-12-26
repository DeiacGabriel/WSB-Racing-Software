DROP DATABASE IF EXISTS WSB_Racing;
CREATE DATABASE WSB_Racing;
USE WSB_Racing;

CREATE TABLE country (
    id BIGINT PRIMARY KEY,
    shorttxt VARCHAR(255) NOT NULL,
    longtxt VARCHAR(255) NOT NULL
);

CREATE TABLE brand (
    id BIGINT PRIMARY KEY,
    name VARCHAR(255) NOT NULL
);

CREATE TABLE customer (
    id BIGINT PRIMARY KEY,
    title VARCHAR(100),
    firstname VARCHAR(255) NOT NULL,
    surname VARCHAR(255) NOT NULL,
    birthdate DATE NOT NULL,
    sex VARCHAR(10),
    mail VARCHAR(255),
    phonenumber VARCHAR(255),
    newsletter BOOLEAN,
    validfrom DATE,
    startnumber VARCHAR(255)
);

CREATE TABLE address (
    id BIGINT PRIMARY KEY,
    customerid BIGINT NOT NULL,
    countryid BIGINT NOT NULL,
    city VARCHAR(255) NOT NULL,
    zip VARCHAR(20) NOT NULL,
    street VARCHAR(255) NOT NULL,
    FOREIGN KEY (customerid) REFERENCES customer(id),
    FOREIGN KEY (countryid) REFERENCES country(id)
);


CREATE TABLE bike (
    id BIGINT PRIMARY KEY,
    brandid BIGINT NOT NULL,
    type VARCHAR(255),
    ccm INT,
    year INT,
    FOREIGN KEY (brandid) REFERENCES brand(id)
);

CREATE TABLE customer_bike (
    id BIGINT PRIMARY KEY,
    customerid BIGINT NOT NULL,
    bikeid BIGINT NOT NULL,
    FOREIGN KEY (customerid) REFERENCES customer(id),
    FOREIGN KEY (bikeid) REFERENCES bike(id)
);

CREATE TABLE cup (
    id BIGINT PRIMARY KEY,
    name VARCHAR(255) NOT NULL
);

CREATE TABLE customer_cup (
    id BIGINT PRIMARY KEY,
    customerid BIGINT NOT NULL,
    cupid BIGINT NOT NULL,
    FOREIGN KEY (customerid) REFERENCES customer(id),
    FOREIGN KEY (cupid) REFERENCES cup(id)
);

CREATE TABLE event (
    id BIGINT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    validfrom DATE NOT NULL,
    validuntil DATE NOT NULL,
    vat FLOAT
);

CREATE TABLE transponder (
    id BIGINT PRIMARY KEY,
    number VARCHAR(255) NOT NULL
);

CREATE TABLE customer_event (
    id BIGINT PRIMARY KEY,
    customerid BIGINT NOT NULL,
    eventid BIGINT NOT NULL,
    bikeid BIGINT NOT NULL,
    transponderid BIGINT NOT NULL,
    laptime TIME,
    FOREIGN KEY (customerid) REFERENCES customer(id),
    FOREIGN KEY (eventid) REFERENCES event(id),
    FOREIGN KEY (bikeid) REFERENCES bike(id),
    FOREIGN KEY (transponderid) REFERENCES transponder(id)
);

CREATE TABLE contact (
    id BIGINT PRIMARY KEY,
    customerid BIGINT NOT NULL,
    firstname VARCHAR(255),
    surname VARCHAR(255),
    phonenumber VARCHAR(20),
    FOREIGN KEY (customerid) REFERENCES customer(id)
);
