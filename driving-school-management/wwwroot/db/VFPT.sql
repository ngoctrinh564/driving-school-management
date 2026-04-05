create or replace PROCEDURE PROC_GET_USERS (
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            u.userId,
            u.userName,
            u.isActive,
            r.roleName,
            hv.hoTen,
            hv.sdt,
            hv.email
        FROM "User" u
        JOIN "Role" r ON u.roleId = r.roleId
        LEFT JOIN HocVien hv ON u.userId = hv.userId
        ORDER BY u.userId;
END;
/

create or replace PROCEDURE PROC_GET_USER_DETAIL (
    p_userId IN NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            u.userId,
            u.userName,
            u.isActive,
            u.roleId,
            r.roleName,
            hv.hocVienId,
            hv.hoTen,
            hv.soCmndCccd,
            hv.namSinh,
            hv.gioiTinh,
            hv.sdt,
            hv.email,
            hv.avatarUrl
        FROM "User" u
        JOIN "Role" r ON u.roleId = r.roleId
        LEFT JOIN HocVien hv ON u.userId = hv.userId
        WHERE u.userId = p_userId;
END;
/

create or replace PROCEDURE PROC_UPDATE_USER
(
    p_userId IN NUMBER,
    p_userName IN NVARCHAR2,
    p_isActive IN NUMBER,
    p_hoTen IN NVARCHAR2,
    p_sdt IN NVARCHAR2,
    p_email IN NVARCHAR2
)
AS
BEGIN
    UPDATE "User"
    SET userName = p_userName,
        isActive = p_isActive
    WHERE userId = p_userId;
END;
/
CREATE OR REPLACE PROCEDURE PROC_UPDATE_USER
(
    p_userId IN NUMBER,
    p_userName IN NVARCHAR2,
    p_isActive IN NUMBER,
    p_hoTen IN NVARCHAR2,
    p_sdt IN NVARCHAR2,
    p_email IN NVARCHAR2
)
AS
BEGIN
    UPDATE "User"
    SET userName = p_userName,
        isActive = p_isActive
    WHERE userId = p_userId;
END;
/

CREATE OR REPLACE PROCEDURE PROC_UPDATE_ROLE_SECURE
(
    p_userId IN NUMBER,
    p_roleId IN NUMBER,
    p_adminUser IN NVARCHAR2,
    p_adminPassword IN NVARCHAR2,
    o_result OUT NUMBER
)
AS
    v_password NVARCHAR2(200);
BEGIN
    -- lấy password admin
    SELECT "password"
    INTO v_password
    FROM "User"
    WHERE userName = p_adminUser;

    -- kiểm tra password (tạm so sánh thẳng)
    IF v_password != p_adminPassword THEN
        o_result := -1;
        RETURN;
    END IF;

    -- update role
    UPDATE "User"
    SET roleId = p_roleId
    WHERE userId = p_userId;

    o_result := 1;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        o_result := -2;
END;
/

CREATE OR REPLACE PROCEDURE PROC_GET_USER_PROFILE
(
    p_userId IN NUMBER,
    o_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN o_cursor FOR
        SELECT
            u.userId      AS USERID,
            hv.hocVienId  AS HOCVIENID,
            u.userName    AS USERNAME,
            hv.email      AS EMAIL,
            hv.hoTen      AS HOTEN,
            hv.soCmndCccd AS SOCMNDCCCD,
            hv.namSinh    AS NAMSINH,
            hv.gioiTinh   AS GIOITINH,
            hv.sdt        AS SDT,
            hv.avatarUrl  AS AVATARURL
        FROM "User" u
        LEFT JOIN HocVien hv ON hv.userId = u.userId
        WHERE u.userId = p_userId;
END;
/

CREATE OR REPLACE PROCEDURE PROC_UPDATE_USER_PROFILE
(
    p_userId       IN NUMBER,
    p_username     IN NVARCHAR2,
    p_email        IN NVARCHAR2,
    p_hoTen        IN NVARCHAR2,
    p_soCmndCccd   IN NVARCHAR2,
    p_namSinh      IN DATE,
    p_gioiTinh     IN NVARCHAR2,
    p_sdt          IN NVARCHAR2,
    p_avatarUrl    IN NVARCHAR2,
    o_result       OUT NUMBER
)
AS
    v_count NUMBER;
    v_user_rows NUMBER;
    v_hocvien_rows NUMBER;
BEGIN
    -- check username trùng người khác
    SELECT COUNT(*) INTO v_count
    FROM "User"
    WHERE userName = p_username
      AND userId <> p_userId;

    IF v_count > 0 THEN
        o_result := -1;
        RETURN;
    END IF;

    -- check email trùng người khác
    SELECT COUNT(*) INTO v_count
    FROM HocVien
    WHERE email = p_email
      AND userId <> p_userId;

    IF v_count > 0 THEN
        o_result := -2;
        RETURN;
    END IF;

    UPDATE "User"
    SET userName = p_username
    WHERE userId = p_userId;

    v_user_rows := SQL%ROWCOUNT;

    UPDATE HocVien
    SET email = p_email,
        hoTen = p_hoTen,
        soCmndCccd = p_soCmndCccd,
        namSinh = p_namSinh,
        gioiTinh = p_gioiTinh,
        sdt = p_sdt,
        avatarUrl = p_avatarUrl
    WHERE userId = p_userId;

    v_hocvien_rows := SQL%ROWCOUNT;

    IF v_user_rows = 0 OR v_hocvien_rows = 0 THEN
        ROLLBACK;
        o_result := 0;
        RETURN;
    END IF;

    COMMIT;
    o_result := 1;

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        o_result := 0;
END;
/

CREATE OR REPLACE VIEW VW_HOSO_THISINH_DANH_SACH AS
SELECT
    HS.hoSoId       AS hoSoId,
    HS.hocVienId    AS hocVienId,
    HV.userId       AS userId,
    HS.hangId       AS hangId,
    HG.maHang       AS maHang,
    HG.tenHang      AS tenHang,
    HS.tenHoSo      AS tenHoSo,
    HS.ngayDangKy   AS ngayDangKy,
    HS.trangThai    AS trangThai,
    HS.ghiChu       AS ghiChu
FROM HoSoThiSinh HS
JOIN HocVien HV ON HS.hocVienId = HV.hocVienId
JOIN HangGplx HG ON HS.hangId = HG.hangId;

CREATE OR REPLACE VIEW VW_BIENBAO_ADMIN AS
SELECT 
    IDBIENBAO,
    TENBIENBAO,
    YNGHIA,
    HINHANH
FROM BIENBAO;
/

CREATE OR REPLACE PROCEDURE PROC_BIENBAO_GET_ALL (
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT IDBIENBAO, TENBIENBAO, YNGHIA, HINHANH
        FROM VW_BIENBAO_ADMIN
        ORDER BY IDBIENBAO;
END;
/

CREATE OR REPLACE PROCEDURE PROC_BIENBAO_GET_BY_ID (
    P_IDBIENBAO IN NUMBER,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT IDBIENBAO, TENBIENBAO, YNGHIA, HINHANH
        FROM VW_BIENBAO_ADMIN
        WHERE IDBIENBAO = P_IDBIENBAO;
END;
/

CREATE OR REPLACE PROCEDURE PROC_BIENBAO_INSERT (
    P_TENBIENBAO IN NVARCHAR2,
    P_YNGHIA IN NVARCHAR2,
    P_HINHANH IN VARCHAR2,
    P_NEW_ID OUT NUMBER
)
AS
BEGIN
    SELECT NVL(MAX(IDBIENBAO), 0) + 1
    INTO P_NEW_ID
    FROM BIENBAO;

    INSERT INTO BIENBAO (
        IDBIENBAO,
        TENBIENBAO,
        YNGHIA,
        HINHANH
    )
    VALUES (
        P_NEW_ID,
        P_TENBIENBAO,
        P_YNGHIA,
        P_HINHANH
    );
END;
/

CREATE OR REPLACE PROCEDURE PROC_BIENBAO_UPDATE (
    P_IDBIENBAO IN NUMBER,
    P_TENBIENBAO IN NVARCHAR2,
    P_YNGHIA IN NVARCHAR2,
    P_HINHANH IN VARCHAR2
)
AS
BEGIN
    UPDATE BIENBAO
    SET 
        TENBIENBAO = P_TENBIENBAO,
        YNGHIA = P_YNGHIA,
        HINHANH = P_HINHANH
    WHERE IDBIENBAO = P_IDBIENBAO;
END;
/


CREATE OR REPLACE PROCEDURE PROC_BIENBAO_DELETE (
    P_IDBIENBAO IN NUMBER
)
AS
BEGIN
    DELETE FROM BIENBAO
    WHERE IDBIENBAO = P_IDBIENBAO;
END;
/

CREATE OR REPLACE PROCEDURE GET_FLASHCARD_SUMMARY (
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            b.IDBIENBAO,
            b.TENBIENBAO,
            b.YNGHIA,
            b.HINHANH,
            COUNT(fc.IDFLASHCARD) AS SODANHGIA
        FROM BIENBAO b
        LEFT JOIN FLASHCARD fc
            ON b.IDBIENBAO = fc.IDBIENBAO
        GROUP BY b.IDBIENBAO, b.TENBIENBAO, b.YNGHIA, b.HINHANH
        ORDER BY b.IDBIENBAO;
END;
/

CREATE OR REPLACE PROCEDURE GET_FLASHCARD_BY_SIGN (
    p_idbienbao IN NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            b.IDBIENBAO,
            b.TENBIENBAO,
            b.YNGHIA,
            b.HINHANH,
            fc.IDFLASHCARD,
            fc.DANHGIA,
            fc.USERID,
            u.USERNAME
        FROM BIENBAO b
        LEFT JOIN FLASHCARD fc
            ON b.IDBIENBAO = fc.IDBIENBAO
        LEFT JOIN "User" u
            ON fc.USERID = u.USERID
        WHERE b.IDBIENBAO = p_idbienbao
        ORDER BY fc.IDFLASHCARD DESC;
END;
/

 
  
COMMIT;