SELECT r."Id" as RoleId, r."Name" AS RoleName, r."Slug" AS RoleSlug, p."Name" AS PermissionName, p."Slug" AS PermissionSlug
FROM "Permission" p
         LEFT JOIN "Role" r ON r."Id" = p."FkRoleId" order by p."DateCreated" desc;

select * from "Tenant" t order by t."DateCreated" desc ;

select * from "User" u order by u."DateCreated" desc ;
select * from "Role" r order by r."DateCreated" desc ;
select * from "Permission" p order by p."DateCreated" desc ;
select * from "UserPermission" up order by up."DateCreated" desc ;
select * from "OneTimePassword" otp  order by otp."DateCreated" desc ;

select * from "UserRole" ur  order by ur."DateCreated" desc ;
select * from "UserPermission" up order by up."DateCreated" desc ;
select * from "Device" d order by d."DateCreated" desc;
select * from "UserPassword" up order by up."DateCreated" desc;



SELECT r."Id" as RoleId, r."Name" AS RoleName, r."Slug" AS RoleSlug, u."Firstname" as FirstName
FROM "UserRole" ur
         LEFT JOIN "Role" r ON r."Id" = ur."FkRoleId"
         RIGHT join "User" u on u."Id" = ur."FkUserId"


SELECT p."Id" as PermissionId, p."Name" AS PermisionName, p."Slug" AS PermissionSlug, u."Firstname" as FirstName
FROM "UserPermission" ur
         LEFT JOIN "Permission" p ON p."Id" = ur."FkPermissionId"
         RIGHT join "User" u on u."Id" = ur."FkUserId"




--TRUNCATE TABLE public."Permission" CONTINUE IDENTITY CASCADE;

