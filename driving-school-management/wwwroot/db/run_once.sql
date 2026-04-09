-- KHÓA HỌC
CREATE SEQUENCE SEQ_KHOAHOC
START WITH 1
INCREMENT BY 1
NOCACHE;
/
ALTER TABLE KetQuaHocTap ADD ( -- thiếu cột, thêm
    DU_DK_THITOTNGHIEP NUMBER(1),
    DAUTOTNGHIEP NUMBER(1),
    DU_DK_THISATHACH NUMBER(1),
    THOIGIANCAPNHAT DATE DEFAULT SYSDATE
);
/
ALTER TABLE KetQuaHocTap RENAME COLUMN soBuoiVang TO soBuoiToiThieu; -- sai tên cột, đổi tên
/
/* Thêm cột KmToiThieu */
ALTER TABLE KetQuaHocTap
ADD KmToiThieu NUMBER;

/* Đổi kiểu soKmHoanThanh sang NUMBER */
ALTER TABLE KetQuaHocTap
MODIFY soKmHoanThanh NUMBER;














commit;