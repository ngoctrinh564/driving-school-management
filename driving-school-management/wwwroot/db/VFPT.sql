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
            hv.HOTEN
        FROM BIENBAO b
        LEFT JOIN FLASHCARD fc
            ON b.IDBIENBAO = fc.IDBIENBAO
        LEFT JOIN "User" u
            ON fc.USERID = u.USERID
        LEFT JOIN HOCVIEN hv
            ON u.USERID = hv.USERID
        WHERE b.IDBIENBAO = p_idbienbao
        ORDER BY fc.IDFLASHCARD DESC;
END;
/

CREATE OR REPLACE PROCEDURE GET_FLASHCARD_DETAIL_BY_SIGN (
    P_IDBIENBAO IN NUMBER,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            b.IDBIENBAO,
            b.TENBIENBAO,
            b.YNGHIA,
            b.HINHANH,
            fc.IDFLASHCARD,
            fc.DANHGIA,
            fc.USERID,
            hv.HOTEN
        FROM BIENBAO b
        INNER JOIN FLASHCARD fc
            ON b.IDBIENBAO = fc.IDBIENBAO
        INNER JOIN "User" u
            ON fc.USERID = u.USERID
        LEFT JOIN HOCVIEN hv
            ON u.USERID = hv.USERID
        WHERE b.IDBIENBAO = P_IDBIENBAO
        ORDER BY fc.IDFLASHCARD DESC;
END;
/
  
CREATE OR REPLACE PROCEDURE SP_GET_HANGGPLX
(
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT HANGID, TENHANG
        FROM HANGGPLX;
END;
/


CREATE OR REPLACE PROCEDURE SP_GET_DANHSACH_HOSO
(
    P_CCCD        IN VARCHAR2 DEFAULT NULL,
    P_TEN         IN VARCHAR2 DEFAULT NULL,
    P_HANG        IN VARCHAR2 DEFAULT NULL,
    P_TRANGTHAI   IN VARCHAR2 DEFAULT NULL,
    P_CURSOR      OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT 
            HS.HOSOID,
            HV.HOTEN,
            HV.SOCMNDCCCD,
            HS.TENHOSO,
            HG.TENHANG,
            HS.NGAYDANGKY,
            HS.TRANGTHAI
        FROM HOSOTHISINH HS
        JOIN HOCVIEN HV ON HS.HOCVIENID = HV.HOCVIENID
        JOIN HANGGPLX HG ON HS.HANGID = HG.HANGID
        WHERE
            (P_CCCD IS NULL OR HV.SOCMNDCCCD LIKE '%' || P_CCCD || '%')
        AND (P_TEN IS NULL OR LOWER(HV.HOTEN) LIKE '%' || LOWER(P_TEN) || '%')
        AND (P_HANG IS NULL OR HG.TENHANG = P_HANG)
        AND (P_TRANGTHAI IS NULL OR HS.TRANGTHAI = P_TRANGTHAI)
        ORDER BY HS.NGAYDANGKY DESC;
END;
/

CREATE OR REPLACE PROCEDURE SP_GET_CHITIET_HOSO
(
    P_HOSOID   IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            HS.HOSOID,
            HS.TENHOSO,
            HS.LOAIHOSO,
            HS.NGAYDANGKY,
            HS.TRANGTHAI,
            HS.GHICHU,

            HV.HOCVIENID,
            HV.HOTEN,
            HV.SOCMNDCCCD,
            HV.NAMSINH,
            HV.GIOITINH,
            HV.SDT,
            HV.EMAIL,
            HV.AVATARURL,

            HG.HANGID,
            HG.MAHANG,
            HG.TENHANG,

            PK.KHAMSUCKHOEID,
            PK.HIEULUC,
            PK.THOIHAN,
            PK.KHAMMAT,
            PK.HUYETAP,
            PK.CHIEUCAO,
            PK.CANNANG
        FROM HOSOTHISINH HS
        JOIN HOCVIEN HV
            ON HS.HOCVIENID = HV.HOCVIENID
        JOIN HANGGPLX HG
            ON HS.HANGID = HG.HANGID
        LEFT JOIN PHIEUKHAMSUCKHOE PK
            ON HS.KHAMSUCKHOEID = PK.KHAMSUCKHOEID
        WHERE HS.HOSOID = P_HOSOID;
END;
/

CREATE OR REPLACE PROCEDURE SP_DUYET_HOSO
(
    P_HOSOID IN NUMBER
)
AS
BEGIN
    UPDATE HOSOTHISINH
    SET TRANGTHAI = 'Đã duyệt'
    WHERE HOSOID = P_HOSOID;
END;
/


CREATE OR REPLACE PROCEDURE SP_TUCHOI_HOSO
(
    P_HOSOID IN NUMBER
)
AS
BEGIN
    UPDATE HOSOTHISINH
    SET TRANGTHAI = 'Từ chối'
    WHERE HOSOID = P_HOSOID;
END;
/


CREATE OR REPLACE PROCEDURE SP_GET_ANH_GKSK_BY_HOSO
(
    P_HOSOID   IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            A.ANHID,
            A.KHAMSUCKHOEID,
            A.URLANH
        FROM HOSOTHISINH HS
        JOIN ANHGKSK A
            ON HS.KHAMSUCKHOEID = A.KHAMSUCKHOEID
        WHERE HS.HOSOID = P_HOSOID
        ORDER BY A.ANHID;
END;
/

CREATE OR REPLACE PACKAGE PKG_ADMIN_REPORT AS
    TYPE REF_CURSOR IS REF CURSOR;

    PROCEDURE GET_OVERVIEW(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    );

    PROCEDURE GET_REVENUE_BY_MONTH(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    );

    PROCEDURE GET_NEW_USERS_BY_MONTH(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    );

    PROCEDURE GET_NEW_COURSES_BY_MONTH(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    );

    PROCEDURE GET_COURSES_BY_HANG(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    );

    PROCEDURE GET_NEW_EXAMS_BY_MONTH(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    );

    PROCEDURE GET_EXAMS_BY_TYPE(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    );
END PKG_ADMIN_REPORT;
/
CREATE OR REPLACE PACKAGE BODY PKG_ADMIN_REPORT AS

    PROCEDURE GET_OVERVIEW(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    ) IS
    BEGIN
        OPEN P_CURSOR FOR
    WITH REV AS (
        SELECT
            NVL(SUM(PT.tongTien), 0) AS TONG_DOANH_THU,
            COUNT(DISTINCT TO_CHAR(PT.ngayLap, 'MM/YYYY')) AS SO_THANG_CO_DOANH_THU
        FROM PhieuThanhToan PT
        WHERE PT.ngayLap IS NOT NULL
          AND (P_FROM_DATE IS NULL OR PT.ngayLap >= TRUNC(P_FROM_DATE))
          AND (P_TO_DATE   IS NULL OR PT.ngayLap < TRUNC(P_TO_DATE) + 1)
    ),
    USERS_NEW AS (
        SELECT COUNT(DISTINCT HS.hocVienId) AS SO_NGUOI_MOI
        FROM HoSoThiSinh HS
        WHERE HS.ngayDangKy IS NOT NULL
          AND (P_FROM_DATE IS NULL OR HS.ngayDangKy >= TRUNC(P_FROM_DATE))
          AND (P_TO_DATE   IS NULL OR HS.ngayDangKy < TRUNC(P_TO_DATE) + 1)
    ),
    COURSES_NEW AS (
        SELECT COUNT(*) AS SO_KHOA_HOC_MOI
        FROM KhoaHoc KH
        WHERE KH.ngayBatDau IS NOT NULL
          AND (P_FROM_DATE IS NULL OR KH.ngayBatDau >= TRUNC(P_FROM_DATE))
          AND (P_TO_DATE   IS NULL OR KH.ngayBatDau < TRUNC(P_TO_DATE) + 1)
    ),
    EXAMS_NEW AS (
        SELECT COUNT(DISTINCT LT.kyThiId) AS SO_KY_THI_MOI
        FROM LichThi LT
        WHERE LT.thoiGianThi IS NOT NULL
          AND (P_FROM_DATE IS NULL OR LT.thoiGianThi >= CAST(TRUNC(P_FROM_DATE) AS TIMESTAMP))
          AND (P_TO_DATE   IS NULL OR LT.thoiGianThi < CAST(TRUNC(P_TO_DATE) + 1 AS TIMESTAMP))
    ),
    HANG_COUNT AS (
        SELECT COUNT(*) AS SO_HANG_GPLX
        FROM HangGplx
    )
    SELECT
        CAST(REV.TONG_DOANH_THU AS NUMBER(18,2)) AS TONG_DOANH_THU,
        CAST(REV.SO_THANG_CO_DOANH_THU AS NUMBER(10,0)) AS SO_THANG_CO_DOANH_THU,
        CAST(
            CASE
                WHEN REV.SO_THANG_CO_DOANH_THU = 0 THEN 0
                ELSE ROUND(REV.TONG_DOANH_THU / REV.SO_THANG_CO_DOANH_THU, 2)
            END
        AS NUMBER(18,2)) AS DOANH_THU_TRUNG_BINH_THANG,
        CAST(USERS_NEW.SO_NGUOI_MOI AS NUMBER(10,0)) AS SO_NGUOI_MOI,
        CAST(COURSES_NEW.SO_KHOA_HOC_MOI AS NUMBER(10,0)) AS SO_KHOA_HOC_MOI,
        CAST(EXAMS_NEW.SO_KY_THI_MOI AS NUMBER(10,0)) AS SO_KY_THI_MOI,
        CAST(HANG_COUNT.SO_HANG_GPLX AS NUMBER(10,0)) AS SO_HANG_GPLX
    FROM REV, USERS_NEW, COURSES_NEW, EXAMS_NEW, HANG_COUNT;
    END GET_OVERVIEW;

    PROCEDURE GET_REVENUE_BY_MONTH(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    ) IS
    BEGIN
        OPEN P_CURSOR FOR
            SELECT
                TO_CHAR(PT.NGAYLAP, 'MM/YYYY') AS LABEL,
                NVL(SUM(PT.TONGTIEN), 0) AS VALUE
            FROM PHIEUTHANHTOAN PT
            WHERE PT.NGAYLAP IS NOT NULL
              AND (P_FROM_DATE IS NULL OR PT.NGAYLAP >= TRUNC(P_FROM_DATE))
              AND (P_TO_DATE   IS NULL OR PT.NGAYLAP < TRUNC(P_TO_DATE) + 1)
            GROUP BY TO_CHAR(PT.NGAYLAP, 'MM/YYYY'), TO_CHAR(PT.NGAYLAP, 'YYYYMM')
            ORDER BY TO_CHAR(PT.NGAYLAP, 'YYYYMM');
    END GET_REVENUE_BY_MONTH;

    PROCEDURE GET_NEW_USERS_BY_MONTH(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    ) IS
    BEGIN
        OPEN P_CURSOR FOR
            SELECT
                TO_CHAR(HS.NGAYDANGKY, 'MM/YYYY') AS LABEL,
                COUNT(DISTINCT HS.HOCVIENID) AS VALUE
            FROM HOSOTHISINH HS
            WHERE HS.NGAYDANGKY IS NOT NULL
              AND (P_FROM_DATE IS NULL OR HS.NGAYDANGKY >= TRUNC(P_FROM_DATE))
              AND (P_TO_DATE   IS NULL OR HS.NGAYDANGKY < TRUNC(P_TO_DATE) + 1)
            GROUP BY TO_CHAR(HS.NGAYDANGKY, 'MM/YYYY'), TO_CHAR(HS.NGAYDANGKY, 'YYYYMM')
            ORDER BY TO_CHAR(HS.NGAYDANGKY, 'YYYYMM');
    END GET_NEW_USERS_BY_MONTH;

    PROCEDURE GET_NEW_COURSES_BY_MONTH(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    ) IS
    BEGIN
        OPEN P_CURSOR FOR
            SELECT
                TO_CHAR(KH.NGAYBATDAU, 'MM/YYYY') AS LABEL,
                COUNT(*) AS VALUE
            FROM KHOAHOC KH
            WHERE KH.NGAYBATDAU IS NOT NULL
              AND (P_FROM_DATE IS NULL OR KH.NGAYBATDAU >= TRUNC(P_FROM_DATE))
              AND (P_TO_DATE   IS NULL OR KH.NGAYBATDAU < TRUNC(P_TO_DATE) + 1)
            GROUP BY TO_CHAR(KH.NGAYBATDAU, 'MM/YYYY'), TO_CHAR(KH.NGAYBATDAU, 'YYYYMM')
            ORDER BY TO_CHAR(KH.NGAYBATDAU, 'YYYYMM');
    END GET_NEW_COURSES_BY_MONTH;

    PROCEDURE GET_COURSES_BY_HANG(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    ) IS
    BEGIN
        OPEN P_CURSOR FOR
            SELECT
                HG.TENHANG AS LABEL,
                COUNT(*) AS VALUE
            FROM KHOAHOC KH
            JOIN HANGGPLX HG ON HG.HANGID = KH.HANGID
            WHERE KH.NGAYBATDAU IS NOT NULL
              AND (P_FROM_DATE IS NULL OR KH.NGAYBATDAU >= TRUNC(P_FROM_DATE))
              AND (P_TO_DATE   IS NULL OR KH.NGAYBATDAU < TRUNC(P_TO_DATE) + 1)
            GROUP BY HG.TENHANG
            ORDER BY COUNT(*) DESC, HG.TENHANG;
    END GET_COURSES_BY_HANG;

    PROCEDURE GET_NEW_EXAMS_BY_MONTH(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    ) IS
    BEGIN
        OPEN P_CURSOR FOR
            SELECT
                TO_CHAR(CAST(LT.THOIGIANTHI AS DATE), 'MM/YYYY') AS LABEL,
                COUNT(DISTINCT LT.KYTHIID) AS VALUE
            FROM LICHTHI LT
            WHERE LT.THOIGIANTHI IS NOT NULL
              AND (P_FROM_DATE IS NULL OR LT.THOIGIANTHI >= CAST(TRUNC(P_FROM_DATE) AS TIMESTAMP))
              AND (P_TO_DATE   IS NULL OR LT.THOIGIANTHI < CAST(TRUNC(P_TO_DATE) + 1 AS TIMESTAMP))
            GROUP BY TO_CHAR(CAST(LT.THOIGIANTHI AS DATE), 'MM/YYYY'),
                     TO_CHAR(CAST(LT.THOIGIANTHI AS DATE), 'YYYYMM')
            ORDER BY TO_CHAR(CAST(LT.THOIGIANTHI AS DATE), 'YYYYMM');
    END GET_NEW_EXAMS_BY_MONTH;

    PROCEDURE GET_EXAMS_BY_TYPE(
        P_FROM_DATE IN DATE,
        P_TO_DATE   IN DATE,
        P_CURSOR    OUT REF_CURSOR
    ) IS
    BEGIN
        OPEN P_CURSOR FOR
            SELECT
                NVL(KT.LOAIKYTHI, 'Khác') AS LABEL,
                COUNT(DISTINCT LT.KYTHIID) AS VALUE
            FROM LICHTHI LT
            JOIN KYTHI KT ON KT.KYTHIID = LT.KYTHIID
            WHERE LT.THOIGIANTHI IS NOT NULL
              AND (P_FROM_DATE IS NULL OR LT.THOIGIANTHI >= CAST(TRUNC(P_FROM_DATE) AS TIMESTAMP))
              AND (P_TO_DATE   IS NULL OR LT.THOIGIANTHI < CAST(TRUNC(P_TO_DATE) + 1 AS TIMESTAMP))
            GROUP BY NVL(KT.LOAIKYTHI, 'Khác')
            ORDER BY COUNT(DISTINCT LT.KYTHIID) DESC, NVL(KT.LOAIKYTHI, 'Khác');
    END GET_EXAMS_BY_TYPE;

END PKG_ADMIN_REPORT;
/





COMMIT;