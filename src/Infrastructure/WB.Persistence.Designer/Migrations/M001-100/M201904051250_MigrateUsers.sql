﻿insert
	into
		plainstore."AspNetUsers" ("Id" ,
		"Email" ,
		"NormalizedEmail" ,
		"EmailConfirmed" ,
		"PasswordHash" ,
		"SecurityStamp" ,
		"TwoFactorEnabled" ,
		"LockoutEnd" ,
		"LockoutEnabled" ,
		"AccessFailedCount" ,
		"UserName" ,
		"NormalizedUserName" ,
		"PasswordSalt" ,
		"CanImportOnHq" ,
		"PhoneNumberConfirmed" ) select
			u.id,
			u.email,
			upper(trim(both ' ' from u.email)),
			u.isconfirmed as "EmailConfirmed",
			u."password" as "PasswordHash",
			--SignInManager.PasswordSignInAsync (used in Login method) 
			--throws an exception http://stackoverflow.com/a/23354148/150342
 uuid_in(md5(random()::text || clock_timestamp()::text)::cstring) as "SecurityStamp",
			false as "TwoFactorEnabled",
			null as "LockoutEndDateUtc",
			u.islockedout as "LockoutEnabled",
			0 as "AccessFailedCount",
			u.username as "UserName",
			upper(trim(both ' ' from u.username)) as "NormalizedUserName",
			u.passwordsalt as "PasswordSalt",
			canimportonhq as "CanImportOnHq",
			false as "PhoneNumberConfirmed"
		from
			plainstore.users u;

insert
	into
		plainstore."AspNetRoles" ("Id",
		"ConcurrencyStamp",
		"Name",
		"NormalizedName")
	values('1',
	null,
	'Administrator',
	'ADMINISTRATOR'),
	('2',
	null,
	'User',
	'USER');

insert
	into
		plainstore."AspNetUserRoles" ("UserId" ,
		"RoleId") select
			userid,
			simpleroleid::text
		from
			plainstore.simpleroles;

insert
	into
		plainstore."AspNetUserClaims" ("ClaimType",
		"ClaimValue",
		"UserId") select
			'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name' as "ClaimType",
			u.fullname as "ClaimValue",
			id as "UserId"
		from
			plainstore.users u
		where
			fullname is not null;
