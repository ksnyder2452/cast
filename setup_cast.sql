use cast_server;
#original
create table if not exists logger(uuid varchar(256) not null, reference_uuid varchar(256), originator varchar(256), type varchar(16), code varchar(16), message varchar(256), original_message varchar(256), event_time_dt DATETIME, display_name varchar(256), filter_on varchar(256), filter_on_owner varchar(256), filter_on_group varchar(256), filter_on_location varchar(256), order_in_system MEDIUMINT NOT NULL AUTO_INCREMENT, primary key(order_in_system));
create table if not exists current_state(uuid varchar(256) not null, reference_uuid varchar(256), state varchar(256), event_time_dt DATETIME, scheduled_time DATETIME, order_in_system MEDIUMINT NOT NULL AUTO_INCREMENT, primary key(order_in_system));
create table if not exists final_results(uuid varchar(256) not null, reference_uuid varchar(256), result varchar(256), event_time_dt DATETIME, order_in_system MEDIUMINT NOT NULL AUTO_INCREMENT, primary key(order_in_system));
create table if not exists client_functionality(uuid varchar(256) not null, reference_uuid varchar(256), start_supported Bool, stop_supported Bool, pause_supported Bool, resume_supported Bool, abort_supported Bool, restart_supported Bool, upload_supported Bool, event_time_dt DATETIME, primary key(uuid));
create table if not exists cast_state_tracker(name varchar(256), state varchar(256), message varchar(256), event_time_dt DATETIME, order_in_system MEDIUMINT NOT NULL AUTO_INCREMENT, primary key(order_in_system));
create table if not exists custom_actions(uuid varchar(256), reference_uuid varchar(256), name varchar(16), event_time_dt DATETIME, order_in_system MEDIUMINT NOT NULL AUTO_INCREMENT, primary key(order_in_system));


#new
create table if not exists logger(uuid varchar(256) not null, reference_uuid varchar(256), originator varchar(256), type varchar(16), code varchar(16), message varchar(256), original_message varchar(256), event_time_dt DATETIME, display_name varchar(256), filter_on varchar(256), filter_on_owner varchar(256), filter_on_group varchar(256), filter_on_location varchar(256), filter_on_keyword varchar(256), order_in_system MEDIUMINT NOT NULL AUTO_INCREMENT, primary key(order_in_system));
create table if not exists current_state(uuid varchar(256) not null, reference_uuid varchar(256), state varchar(256), event_time_dt DATETIME, scheduled_time DATETIME, order_in_system MEDIUMINT NOT NULL AUTO_INCREMENT, primary key(order_in_system));
create table if not exists final_results(uuid varchar(256) not null, reference_uuid varchar(256), result varchar(256), event_time_dt DATETIME, order_in_system MEDIUMINT NOT NULL AUTO_INCREMENT, primary key(order_in_system));
create table if not exists client_functionality(uuid varchar(256) not null, reference_uuid varchar(256), start_supported Bool, stop_supported Bool, pause_supported Bool, resume_supported Bool, abort_supported Bool, restart_supported Bool, upload_supported Bool, event_time_dt DATETIME, primary key(uuid));
create table if not exists cast_state_tracker(name varchar(256), state varchar(256), message varchar(256), event_time_dt DATETIME, order_in_system MEDIUMINT NOT NULL AUTO_INCREMENT, primary key(order_in_system));
create table if not exists custom_actions(uuid varchar(256), reference_uuid varchar(256), name varchar(16), event_time_dt DATETIME, order_in_system MEDIUMINT NOT NULL AUTO_INCREMENT, primary key(order_in_system));

create user 'cast_read'@'172.17.0.1' identified by 'my_cast_pwd_02';
create user 'cast_write'@'172.17.0.1' identified by 'my_cast_pwd_03';
grant SELECT on cast_server.* to 'cast_read'@'172.17.0.1';
grant INSERT, UPDATE, DELETE on cast_server.* to 'cast_write'@'172.17.0.1';
commit;

desc logger;
desc current_state;
desc final_results;
desc client_functionality;
desc cast_state_tracker;
desc custom_actions;
