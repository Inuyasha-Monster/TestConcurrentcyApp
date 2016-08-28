alter proc optional_parameter
@name nvarchar(50)=null
as
if(@name is not null)
 select st.Name from dbo.Student as st where st.Name like @name+'%';
else
	select st.Name from dbo.Student as  st

exec dbo.optional_parameter



