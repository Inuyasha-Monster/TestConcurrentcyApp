select sc.StudentID as 学生编号,st.Name as 学生姓名, avg(sc.Score) as 平均成绩 from Score as sc
inner join [dbo].[Student] as st on sc.StudentID= st.ID
group by sc.StudentID,st.Name having AVG(sc.Score)>60
