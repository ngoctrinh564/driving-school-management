-- ============================================================
-- ====================        AUTH        ====================
-- ============================================================
CREATE OR REPLACE PROCEDURE PROC_LOGIN
(
    p_username IN NVARCHAR2,
    o_userId OUT NUMBER,
    o_roleId OUT NUMBER,
    o_username OUT NVARCHAR2,
    o_password OUT NVARCHAR2,
    o_isActive OUT NUMBER
)
AS
BEGIN
    SELECT userId, roleId, userName, "password", isActive
    INTO o_userId, o_roleId, o_username, o_password, o_isActive
    FROM "User"
    WHERE userName = p_username;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        o_userId := NULL;
END;
/

CREATE OR REPLACE PROCEDURE PROC_REGISTER
(
    p_username IN NVARCHAR2,
    p_password IN NVARCHAR2,
    p_roleId IN NUMBER,
    o_result OUT NUMBER
)
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM "User"
    WHERE userName = p_username;

    IF v_count > 0 THEN
        o_result := -1;
        RETURN;
    END IF;

    INSERT INTO "User"(roleId, userName, "password", isActive)
    VALUES (p_roleId, p_username, p_password, 1);

    o_result := 1;
END;
/

CREATE OR REPLACE PROCEDURE PROC_RESET_PASSWORD
(
    p_username IN NVARCHAR2,
    p_newPassword IN NVARCHAR2,
    o_result OUT NUMBER
)
AS
BEGIN
    UPDATE "User"
    SET "password" = p_newPassword
    WHERE userName = p_username;

    IF SQL%ROWCOUNT = 0 THEN
        o_result := -1;
    ELSE
        o_result := 1;
    END IF;
END;
/

-- kiểm tra đã có những thông tin cần thiết trước khi đăng nhập vào
CREATE OR REPLACE FUNCTION FUNC_CHECK_USER_PROFILE
(
    p_userId IN NUMBER
)
RETURN NUMBER
AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM HOCVIEN
    WHERE USERID = p_userId
      AND EMAIL IS NOT NULL
      AND SDT IS NOT NULL
      AND SOCMNDCCCD IS NOT NULL;

    IF v_count = 0 THEN
        RETURN 0; -- chưa đủ
    END IF;

    RETURN 1; -- đầy đủ
END;
/
-- ============================================================
-- ====================        ADMIN        ===================
-- ============================================================
-- DASHBOARD
CREATE OR REPLACE PROCEDURE PROC_ADMIN_DASHBOARD
(
    o_totalUser OUT NUMBER,
    o_totalHoSo OUT NUMBER,
    o_totalGPLX OUT NUMBER,
    o_totalBaiThi OUT NUMBER,
    o_userActive OUT NUMBER,
    o_userInactive OUT NUMBER,
    o_recentUsers OUT SYS_REFCURSOR
)
AS
BEGIN
    -- tổng user
    SELECT COUNT(*) INTO o_totalUser FROM "User";
    -- tổng hồ sơ
    SELECT COUNT(*) INTO o_totalHoSo FROM HoSoThiSinh;
    -- tổng GPLX
    SELECT COUNT(*) INTO o_totalGPLX FROM GiayPhepLaiXe;
    -- tổng bài thi
    SELECT COUNT(*) INTO o_totalBaiThi FROM BaiThi;
    -- user active
    SELECT COUNT(*) INTO o_userActive FROM "User" WHERE isActive = 1;
    -- user inactive
    SELECT COUNT(*) INTO o_userInactive FROM "User" WHERE isActive = 0;
    -- 5 user mới nhất
    OPEN o_recentUsers FOR
        SELECT *
        FROM (
            SELECT userId, userName, isActive
            FROM "User"
            ORDER BY userId DESC
        )
        WHERE ROWNUM <= 5;

END;
/
-- Exam: kì thi
CREATE OR REPLACE VIEW VW_KYTHI_ADMIN AS
SELECT 
    kt.kyThiId,
    kt.tenKyThi,
    kt.loaiKyThi,
    COUNT(ct.hoSoId) AS soLuongDangKy
FROM KyThi kt
LEFT JOIN ChiTietDangKyThi ct 
    ON kt.kyThiId = ct.kyThiId
GROUP BY 
    kt.kyThiId, 
    kt.tenKyThi, 
    kt.loaiKyThi;
    
/
CREATE OR REPLACE PROCEDURE SP_CREATE_KYTHI
(
    p_tenKyThi   KyThi.tenKyThi%TYPE,
    p_loaiKyThi  KyThi.loaiKyThi%TYPE
)
AS
BEGIN
    INSERT INTO KyThi (tenKyThi, loaiKyThi)
    VALUES (p_tenKyThi, p_loaiKyThi);
END;
/

CREATE OR REPLACE PROCEDURE SP_UPDATE_KYTHI
(
    p_kyThiId NUMBER,
    p_tenKyThi NVARCHAR2,
    p_loaiKyThi NVARCHAR2
)
AS
BEGIN
    UPDATE KyThi
    SET 
        tenKyThi = p_tenKyThi,
        loaiKyThi = p_loaiKyThi
    WHERE kyThiId = p_kyThiId;
END;
/
CREATE OR REPLACE PROCEDURE SP_DELETE_KYTHI
(
    p_kyThiId NUMBER
)
AS
BEGIN
    DELETE FROM KyThi
    WHERE kyThiId = p_kyThiId;
END;
/
CREATE OR REPLACE PROCEDURE SP_GET_KYTHI
(
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
    SELECT * FROM VW_KYTHI_ADMIN
    ORDER BY kyThiId DESC;
END;
/
CREATE OR REPLACE PROCEDURE SP_CREATE_LICHTHI
(
    p_kyThiId NUMBER,
    p_thoiGianThi TIMESTAMP,
    p_diaDiem NVARCHAR2
)
AS
BEGIN
    INSERT INTO LichThi (kyThiId, thoiGianThi, diaDiem)
    VALUES (p_kyThiId, p_thoiGianThi, p_diaDiem);
END;
/
CREATE OR REPLACE PROCEDURE SP_GET_LICHTHI_BY_KYTHI
(
    p_kyThiId NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
    SELECT *
    FROM LichThi
    WHERE kyThiId = p_kyThiId
    ORDER BY thoiGianThi;
END;
/



-- ============================================================
-- ====================        USER        ====================
-- ============================================================
-- HOME
CREATE OR REPLACE PROCEDURE SP_HOME_DASHBOARD (
    p_user_id              IN NUMBER,
    p_total_courses        OUT NUMBER,
    p_total_exams          OUT NUMBER,
    p_total_licenses       OUT NUMBER,
    p_featured_courses     OUT SYS_REFCURSOR,
    p_my_profiles          OUT SYS_REFCURSOR,
    p_my_exam_results      OUT SYS_REFCURSOR
)
AS
BEGIN
    /* ================= TỔNG QUAN CHUNG ================= */

    SELECT COUNT(*)
    INTO p_total_courses
    FROM KhoaHoc;

    SELECT COUNT(*)
    INTO p_total_exams
    FROM KyThi;

    SELECT COUNT(*)
    INTO p_total_licenses
    FROM GiayPhepLaiXe;

    /* ================= KHÓA HỌC NỔI BẬT ================= */

    OPEN p_featured_courses FOR
        SELECT
            kh.khoaHocId,
            kh.tenKhoaHoc,
            hg.tenHang,
            kh.ngayBatDau,
            kh.ngayKetThuc,
            kh.diaDiem,
            kh.trangThai,
            hg.hocPhi
        FROM KhoaHoc kh
        JOIN HangGplx hg ON hg.hangId = kh.hangId
        ORDER BY kh.ngayBatDau DESC
        FETCH FIRST 6 ROWS ONLY;

    /* ================= NẾU CHƯA ĐĂNG NHẬP ================= */

    IF p_user_id IS NULL OR p_user_id = 0 THEN

        OPEN p_my_profiles FOR
            SELECT
                CAST(NULL AS NUMBER) AS hoSoId,
                CAST(NULL AS NVARCHAR2(100)) AS tenHoSo,
                CAST(NULL AS NVARCHAR2(20)) AS tenHang,
                CAST(NULL AS DATE) AS ngayDangKy,
                CAST(NULL AS NVARCHAR2(50)) AS trangThai
            FROM dual
            WHERE 1 = 0;

        OPEN p_my_exam_results FOR
            SELECT
                CAST(NULL AS NVARCHAR2(100)) AS tenBaiThi,
                CAST(NULL AS NVARCHAR2(50)) AS ketQuaDatDuoc,
                CAST(NULL AS BINARY_FLOAT) AS tongDiem,
                CAST(NULL AS NVARCHAR2(100)) AS tenKyThi
            FROM dual
            WHERE 1 = 0;

    ELSE

        /* ================= HỒ SƠ CỦA USER ================= */

        OPEN p_my_profiles FOR
            SELECT
                hs.hoSoId,
                hs.tenHoSo,
                hg.tenHang,
                hs.ngayDangKy,
                hs.trangThai
            FROM HocVien hv
            JOIN HoSoThiSinh hs ON hs.hocVienId = hv.hocVienId
            JOIN HangGplx hg ON hg.hangId = hs.hangId
            WHERE hv.userId = p_user_id
            ORDER BY hs.ngayDangKy DESC;

        /* ================= KẾT QUẢ THI CỦA USER ================= */

        OPEN p_my_exam_results FOR
            SELECT
                bt.tenBaiThi,
                ct.ketQuaDatDuoc,
                ct.tongDiem,
                kt.tenKyThi
            FROM HocVien hv
            JOIN HoSoThiSinh hs ON hs.hocVienId = hv.hocVienId
            JOIN ChiTietKetQuaThi ct ON ct.hoSoId = hs.hoSoId
            JOIN BaiThi bt ON bt.baiThiId = ct.baiThiId
            JOIN KyThi kt ON kt.kyThiId = bt.kyThiId
            WHERE hv.userId = p_user_id
            ORDER BY bt.baiThiId DESC
            FETCH FIRST 5 ROWS ONLY;

    END IF;

END;
/

-- KHÓA HỌC
CREATE OR REPLACE PROCEDURE sp_GetKhoaHocDangMo (
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            kh.khoaHocId,
            kh.tenKhoaHoc,
            kh.ngayBatDau,
            kh.ngayKetThuc,
            kh.diaDiem,
            kh.trangThai,
            hg.tenHang,
            hg.hocPhi
        FROM KhoaHoc kh
        JOIN HangGplx hg ON kh.hangId = hg.hangId
        WHERE kh.trangThai IN (N'Sắp khai giảng', N'Đang học')
        ORDER BY kh.ngayBatDau ASC;
END;
/
CREATE OR REPLACE PROCEDURE sp_GetKhoaHocDetail (
    p_khoaHocId IN NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            kh.khoaHocId,
            kh.tenKhoaHoc,
            kh.ngayBatDau,
            kh.ngayKetThuc,
            kh.diaDiem,
            kh.trangThai,
            hg.hangId,
            hg.tenHang,
            hg.moTa,
            hg.loaiPhuongTien,
            hg.soCauHoi,
            hg.diemDat,
            hg.thoiGianTn,
            hg.hocPhi
        FROM KhoaHoc kh
        JOIN HangGplx hg ON kh.hangId = hg.hangId
        WHERE kh.khoaHocId = p_khoaHocId;
END;
/
-- HỌC DASHBOASRD
CREATE OR REPLACE PROCEDURE PR_HOC_INDEX (
    P_MAHANG   IN VARCHAR2,
    P_USERID   IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
    V_MAHANG   VARCHAR2(10);
BEGIN
    V_MAHANG := UPPER(TRIM(P_MAHANG));

    OPEN P_CURSOR FOR
        WITH HANG_INFO AS (
            SELECT
                HG.HANGID,
                TRIM(UPPER(HG.MAHANG)) AS MAHANG,
                HG.THOIGIANTN,
                HG.SOCAUHOI
            FROM HANGGPLX HG
            WHERE TRIM(UPPER(HG.MAHANG)) = V_MAHANG
        ),
        FLAG_INFO AS (
            SELECT
                CASE
                    WHEN V_MAHANG IN ('A', 'A1') THEN 1
                    ELSE 0
                END AS IS_XEMAY
            FROM DUAL
        )
        SELECT
            H.HANGID,
            H.MAHANG,
            H.THOIGIANTN,
            (H.THOIGIANTN / 60) AS THOIGIANTHI,
            H.SOCAUHOI AS SOCAUTHINGAUNHIEN,

            (
                SELECT COUNT(*)
                FROM BODETHITHU B
                WHERE B.HANGID = H.HANGID
                  AND B.HOATDONG = 1
            ) AS TOTAL_BODE,

            CASE
                WHEN P_USERID IS NULL THEN 0
                ELSE (
                    SELECT COUNT(*)
                    FROM BAILAM BL
                    JOIN BODETHITHU B ON B.BODEID = BL.BODEID
                    WHERE BL.USERID = P_USERID
                      AND B.HANGID = H.HANGID
                )
            END AS DONE_BODE,

            (
                SELECT COUNT(*)
                FROM CAUHOILYTHUYET CH, FLAG_INFO F
                WHERE (F.IS_XEMAY = 0 OR CH.XEMAY = 1)
            ) AS TOTAL_CAUHOI,

            (
                SELECT COUNT(*)
                FROM CAUHOILYTHUYET CH, FLAG_INFO F
                WHERE CH.CAULIET = 1
                  AND (F.IS_XEMAY = 0 OR CH.XEMAY = 1)
            ) AS TOTAL_CAULIET,

            (
                SELECT COUNT(*)
                FROM CAUHOILYTHUYET CH, FLAG_INFO F
                WHERE CH.CHUY = 1
                  AND (F.IS_XEMAY = 0 OR CH.XEMAY = 1)
            ) AS TOTAL_CAUCHUY,

            (
                SELECT COUNT(*)
                FROM BIENBAO
            ) AS TOTAL_BIENBAO,

            CASE
                WHEN V_MAHANG IN ('A', 'A1') THEN 0
                ELSE 1
            END AS HAS_MOPHONG,

            CASE
                WHEN V_MAHANG IN ('A', 'A1') THEN 0
                ELSE (
                    SELECT COUNT(*)
                    FROM BODEMOPHONG BMP
                    WHERE BMP.ISACTIVE = 1
                )
            END AS MP_BODE,

            CASE
                WHEN V_MAHANG IN ('A', 'A1') THEN 0
                ELSE (
                    SELECT COUNT(*)
                    FROM TINHHUONGMOPHONG
                )
            END AS MP_TINHHUONG,

            CASE
                WHEN V_MAHANG IN ('A', 'A1') OR P_USERID IS NULL THEN 0
                ELSE (
                    SELECT COUNT(*)
                    FROM BAILAMMOPHONG BLM
                    JOIN BODEMOPHONG BMP ON BMP.BODEMOPHONGID = BLM.BODEMOPHONGID
                    WHERE BLM.USERID = P_USERID
                      AND BMP.ISACTIVE = 1
                )
            END AS MP_BODE_DONE
        FROM HANG_INFO H;
END;
/
CREATE OR REPLACE PROCEDURE PR_HOC_CAU_LIET (
    P_MAHANG   IN VARCHAR2,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
    V_MAHANG VARCHAR2(10);
BEGIN
    V_MAHANG := UPPER(TRIM(P_MAHANG));

    OPEN P_CURSOR FOR
        WITH QUESTION_BASE AS (
            SELECT
                CH.CAUHOIID,
                CH.CHUONGID,
                CH.NOIDUNG,
                CH.HINHANH,
                CH.CAULIET,
                CH.CHUY,
                CH.XEMAY,
                CH.URLANHMEO,
                C.TENCHUONG,
                NVL(C.THUTU, 0) AS CHUONG_THUTU,
                ROW_NUMBER() OVER (
                    ORDER BY NVL(C.THUTU, 0), CH.CAUHOIID
                ) AS GLOBAL_INDEX
            FROM CAUHOILYTHUYET CH
            JOIN CHUONG C ON C.CHUONGID = CH.CHUONGID
        )
        SELECT
            Q.GLOBAL_INDEX,
            Q.CAUHOIID,
            Q.NOIDUNG,
            Q.HINHANH,
            Q.URLANHMEO,
            Q.CAULIET,
            Q.CHUY,
            Q.XEMAY,
            Q.CHUONGID,
            Q.TENCHUONG,
            Q.CHUONG_THUTU,
            D.DAPANID,
            D.THUTU AS DAPAN_THUTU,
            D.DAPANDUNG
        FROM QUESTION_BASE Q
        JOIN DAPAN D ON D.CAUHOIID = Q.CAUHOIID
        WHERE Q.CAULIET = 1
          AND (
                V_MAHANG NOT IN ('A', 'A1')
                OR Q.XEMAY = 1
              )
        ORDER BY Q.CHUONG_THUTU, Q.CAUHOIID, D.THUTU;
END;
/
CREATE OR REPLACE PROCEDURE PR_HOC_CHU_Y (
    P_MAHANG   IN VARCHAR2,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
    V_MAHANG VARCHAR2(10);
BEGIN
    V_MAHANG := UPPER(TRIM(P_MAHANG));

    OPEN P_CURSOR FOR
        WITH QUESTION_BASE AS (
            SELECT
                CH.CAUHOIID,
                CH.CHUONGID,
                CH.NOIDUNG,
                CH.HINHANH,
                CH.CAULIET,
                CH.CHUY,
                CH.XEMAY,
                CH.URLANHMEO,
                C.TENCHUONG,
                NVL(C.THUTU, 0) AS CHUONG_THUTU,
                ROW_NUMBER() OVER (
                    ORDER BY NVL(C.THUTU, 0), CH.CAUHOIID
                ) AS GLOBAL_INDEX
            FROM CAUHOILYTHUYET CH
            JOIN CHUONG C ON C.CHUONGID = CH.CHUONGID
        )
        SELECT
            Q.GLOBAL_INDEX,
            Q.CAUHOIID,
            Q.NOIDUNG,
            Q.HINHANH,
            Q.URLANHMEO,
            Q.CAULIET,
            Q.CHUY,
            Q.XEMAY,
            Q.CHUONGID,
            Q.TENCHUONG,
            Q.CHUONG_THUTU,
            D.DAPANID,
            D.THUTU AS DAPAN_THUTU,
            D.DAPANDUNG
        FROM QUESTION_BASE Q
        JOIN DAPAN D ON D.CAUHOIID = Q.CAUHOIID
        WHERE Q.CHUY = 1
          AND (
                V_MAHANG NOT IN ('A', 'A1')
                OR Q.XEMAY = 1
              )
        ORDER BY Q.CHUONG_THUTU, Q.CAUHOIID, D.THUTU;
END;
/
CREATE OR REPLACE PROCEDURE PR_FLASHCARD_BIENBAO (
    P_USERID   IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            B.IDBIENBAO,
            B.TENBIENBAO,
            B.YNGHIA,
            B.HINHANH,
            F.IDFLASHCARD,
            F.DANHGIA
        FROM BIENBAO B
        LEFT JOIN FLASHCARD F
               ON F.IDBIENBAO = B.IDBIENBAO
              AND F.USERID = P_USERID
        ORDER BY B.IDBIENBAO;
END;
/
CREATE OR REPLACE PROCEDURE PR_FLASHCARD_SAVE (
    P_USERID      IN NUMBER,
    P_IDBIENBAO   IN NUMBER,
    P_DANHGIA     IN NVARCHAR2
)
AS
    V_COUNT NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO V_COUNT
    FROM FLASHCARD
    WHERE USERID = P_USERID
      AND IDBIENBAO = P_IDBIENBAO;

    IF V_COUNT = 0 THEN
        INSERT INTO FLASHCARD (DANHGIA, USERID, IDBIENBAO)
        VALUES (P_DANHGIA, P_USERID, P_IDBIENBAO);
    ELSE
        UPDATE FLASHCARD
        SET DANHGIA = P_DANHGIA
        WHERE USERID = P_USERID
          AND IDBIENBAO = P_IDBIENBAO;
    END IF;
END;
/
CREATE OR REPLACE PROCEDURE PR_GET_HANG
(
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT MAHANG FROM HANGGPLX ORDER BY MAHANG;
END;
/
-- LÝ THUYẾT
CREATE OR REPLACE VIEW VW_BODE_LYTHUYET AS
SELECT 
    B.BODEID,
    B.TENBODE,
    NVL(B.THOIGIAN, H.THOIGIANTN) AS THOIGIAN,
    NVL(B.SOCAUHOI, H.SOCAUHOI) AS SOCAUHOI,
    B.HANGID,
    H.MAHANG,
    H.DIEMDAT,
    H.THOIGIANTN
FROM BODETHITHU B
JOIN HANGGPLX H ON H.HANGID = B.HANGID
WHERE B.HOATDONG = 1;
/

CREATE OR REPLACE PROCEDURE PR_GET_BODE_BY_HANG
(
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT *
    FROM VW_BODE_LYTHUYET
    WHERE UPPER(TRIM(MAHANG)) = UPPER(TRIM(P_MAHANG))
    ORDER BY BODEID;
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_EXAM_DETAIL
(
    P_BODEID IN NUMBER,
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT 
        B.BODEID,
        B.TENBODE,
        NVL(B.THOIGIAN, H.THOIGIANTN) AS THOIGIAN,
        NVL(B.SOCAUHOI, H.SOCAUHOI) AS SOCAUHOI,
        H.DIEMDAT,
        TRIM(H.MAHANG) AS MAHANG,
        CH.CAUHOIID,
        CH.NOIDUNG,
        CH.HINHANH,
        CH.URLANHMEO,
        NVL(CH.CAULIET, 0) AS CAULIET,
        D.DAPANID,
        D.THUTU,
        NVL(D.DAPANDUNG, 0) AS DAPANDUNG
    FROM BODETHITHU B
    JOIN HANGGPLX H ON H.HANGID = B.HANGID
    JOIN CHITIETBODETRACNGHIEM CT ON CT.BODEID = B.BODEID
    JOIN CAUHOILYTHUYET CH ON CH.CAUHOIID = CT.CAUHOIID
    JOIN DAPAN D ON D.CAUHOIID = CH.CAUHOIID
    WHERE B.BODEID = P_BODEID
      AND B.HOATDONG = 1
      AND UPPER(TRIM(H.MAHANG)) = UPPER(TRIM(P_MAHANG))
    ORDER BY NVL(CT.THUTU, 999999), CH.CAUHOIID, D.THUTU;
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_HISTORY_EXAM
(
    P_USERID IN NUMBER,
    P_BODEID IN NUMBER,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT 
        BL.BAILAMID,
        BL.THOIGIANLAMBAI,
        BL.SOCAUSAI,
        NVL(BL.KETQUA, 0) AS KETQUA,
        CT.CAUHOIID,
        CT.DAPANDACHON,
        NVL(CT.KETQUACAU, 0) AS KETQUACAU
    FROM BAILAM BL
    JOIN CHITIETBAILAM CT ON CT.BAILAMID = BL.BAILAMID
    WHERE BL.BAILAMID = (
        SELECT MAX(BL2.BAILAMID)
        FROM BAILAM BL2
        WHERE BL2.USERID = P_USERID
          AND BL2.BODEID = P_BODEID
    )
    ORDER BY CT.CAUHOIID;
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_LAST_BAILAM_BY_HANG
(
    P_USERID IN NUMBER,
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT 
        X.BODEID,
        X.BAILAMID,
        X.THOIGIANLAMBAI,
        X.SOCAUSAI,
        X.KETQUA
    FROM
    (
        SELECT
            BL.BODEID,
            BL.BAILAMID,
            BL.THOIGIANLAMBAI,
            BL.SOCAUSAI,
            NVL(BL.KETQUA, 0) AS KETQUA,
            ROW_NUMBER() OVER (PARTITION BY BL.BODEID ORDER BY BL.BAILAMID DESC) AS RN
        FROM BAILAM BL
        JOIN BODETHITHU B ON B.BODEID = BL.BODEID
        JOIN HANGGPLX H ON H.HANGID = B.HANGID
        WHERE BL.USERID = P_USERID
          AND UPPER(TRIM(H.MAHANG)) = UPPER(TRIM(P_MAHANG))
    ) X
    WHERE X.RN = 1;
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_HANG_INFO
(
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
    SELECT
        HANGID,
        MAHANG,
        SOCAUHOI,
        DIEMDAT,
        THOIGIANTN
    FROM HANGGPLX
    WHERE UPPER(TRIM(MAHANG)) = UPPER(TRIM(P_MAHANG));
END;
/

CREATE OR REPLACE PROCEDURE PR_GET_ALL_QUESTION_FOR_HANG
(
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
    V_IS_XEMAY NUMBER := 0;
BEGIN
    IF UPPER(TRIM(P_MAHANG)) IN ('A', 'A1') THEN
        V_IS_XEMAY := 1;
    END IF;

    OPEN P_CURSOR FOR
    WITH QUESTION_BASE AS
    (
        SELECT
            ROW_NUMBER() OVER (ORDER BY NVL(C.THUTU, 999999), CH.CAUHOIID) AS GLOBAL_INDEX,
            CH.CAUHOIID,
            CH.NOIDUNG,
            CH.HINHANH,
            CH.URLANHMEO,
            NVL(CH.CAULIET, 0) AS CAULIET,
            NVL(CH.CHUY, 0) AS CHUY,
            NVL(CH.XEMAY, 0) AS XEMAY,
            C.CHUONGID,
            C.TENCHUONG,
            NVL(C.THUTU, 999999) AS CHUONGTHUTU,
            D.DAPANID,
            D.THUTU AS DAPANTHUTU,
            NVL(D.DAPANDUNG, 0) AS DAPANDUNG
        FROM CAUHOILYTHUYET CH
        JOIN CHUONG C ON C.CHUONGID = CH.CHUONGID
        JOIN DAPAN D ON D.CAUHOIID = CH.CAUHOIID
        WHERE (V_IS_XEMAY = 0 OR NVL(CH.XEMAY, 0) = 1)
    )
    SELECT *
    FROM QUESTION_BASE
    ORDER BY CHUONGTHUTU, CAUHOIID, DAPANTHUTU;
END;
/

CREATE OR REPLACE PROCEDURE PR_RANDOM_EXAM
(
    P_MAHANG IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
    V_HANGID NUMBER;
    V_SOCAU NUMBER;
    V_DIEMDAT NUMBER;
    V_THOIGIANTN NUMBER;
    V_IS_XEMAY NUMBER := 0;
BEGIN
    SELECT HANGID, SOCAUHOI, DIEMDAT, THOIGIANTN
    INTO V_HANGID, V_SOCAU, V_DIEMDAT, V_THOIGIANTN
    FROM HANGGPLX
    WHERE UPPER(TRIM(MAHANG)) = UPPER(TRIM(P_MAHANG));

    IF UPPER(TRIM(P_MAHANG)) IN ('A', 'A1') THEN
        V_IS_XEMAY := 1;
    END IF;

    OPEN P_CURSOR FOR
    WITH QBASE AS
    (
        SELECT
            CH.CAUHOIID,
            CH.NOIDUNG,
            CH.HINHANH,
            CH.URLANHMEO,
            NVL(CH.CAULIET, 0) AS CAULIET
        FROM CAUHOILYTHUYET CH
        WHERE (V_IS_XEMAY = 0 OR NVL(CH.XEMAY, 0) = 1)
    ),
    RANDQ AS
    (
        SELECT *
        FROM
        (
            SELECT Q.*, DBMS_RANDOM.VALUE AS RND
            FROM QBASE Q
        )
        ORDER BY RND
        FETCH FIRST V_SOCAU ROWS ONLY
    )
    SELECT
        -1 AS BODEID,
        N'Đề thi ngẫu nhiên hạng ' || P_MAHANG AS TENBODE,
        V_THOIGIANTN AS THOIGIAN,
        V_SOCAU AS SOCAUHOI,
        V_DIEMDAT AS DIEMDAT,
        P_MAHANG AS MAHANG,
        R.CAUHOIID,
        R.NOIDUNG,
        R.HINHANH,
        R.URLANHMEO,
        R.CAULIET,
        D.DAPANID,
        D.THUTU,
        NVL(D.DAPANDUNG, 0) AS DAPANDUNG
    FROM RANDQ R
    JOIN DAPAN D ON D.CAUHOIID = R.CAUHOIID
    ORDER BY R.CAUHOIID, D.THUTU;
END;
/

CREATE OR REPLACE PROCEDURE PR_SAVE_BAILAM
(
    P_USERID IN NUMBER,
    P_BODEID IN NUMBER,
    P_THOIGIAN IN NUMBER,
    P_SOCAUSAI IN NUMBER,
    P_KETQUA IN NUMBER,
    P_BAILAMID OUT NUMBER
)
AS
BEGIN
    INSERT INTO BAILAM(USERID, BODEID, THOIGIANLAMBAI, SOCAUSAI, KETQUA)
    VALUES (P_USERID, P_BODEID, P_THOIGIAN, P_SOCAUSAI, P_KETQUA)
    RETURNING BAILAMID INTO P_BAILAMID;
END;
/

CREATE OR REPLACE PROCEDURE PR_SAVE_CT_BAILAM
(
    P_BAILAMID IN NUMBER,
    P_CAUHOIID IN NUMBER,
    P_DAPAN IN NVARCHAR2,
    P_KETQUA IN NUMBER
)
AS
BEGIN
    INSERT INTO CHITIETBAILAM(BAILAMID, CAUHOIID, DAPANDACHON, KETQUACAU)
    VALUES (P_BAILAMID, P_CAUHOIID, P_DAPAN, P_KETQUA);
END;
/

CREATE OR REPLACE FUNCTION FUNC_CHAM_DIEM
(
    P_BAILAMID IN NUMBER
)
RETURN NUMBER
AS
    V_DUNG NUMBER := 0;
BEGIN
    SELECT COUNT(*)
    INTO V_DUNG
    FROM CHITIETBAILAM
    WHERE BAILAMID = P_BAILAMID
      AND KETQUACAU = 1;

    RETURN V_DUNG;
END;
/
CREATE OR REPLACE PROCEDURE PR_GET_RANDOM_EXAM_BY_IDS
(
    P_MAHANG IN VARCHAR2,
    P_IDS IN VARCHAR2,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
    V_DIEMDAT NUMBER;
    V_THOIGIANTN NUMBER;
BEGIN
    SELECT DIEMDAT, THOIGIANTN
    INTO V_DIEMDAT, V_THOIGIANTN
    FROM HANGGPLX
    WHERE UPPER(TRIM(MAHANG)) = UPPER(TRIM(P_MAHANG));

    OPEN P_CURSOR FOR
    WITH ID_LIST AS
    (
        SELECT TO_NUMBER(REGEXP_SUBSTR(P_IDS, '[^,]+', 1, LEVEL)) AS CAUHOIID
        FROM DUAL
        CONNECT BY REGEXP_SUBSTR(P_IDS, '[^,]+', 1, LEVEL) IS NOT NULL
    )
    SELECT
        -1 AS BODEID,
        N'Đề thi ngẫu nhiên hạng ' || P_MAHANG AS TENBODE,
        V_THOIGIANTN AS THOIGIAN,
        (SELECT COUNT(*) FROM ID_LIST) AS SOCAUHOI,
        V_DIEMDAT AS DIEMDAT,
        P_MAHANG AS MAHANG,
        CH.CAUHOIID,
        CH.NOIDUNG,
        CH.HINHANH,
        CH.URLANHMEO,
        NVL(CH.CAULIET, 0) AS CAULIET,
        D.DAPANID,
        D.THUTU,
        NVL(D.DAPANDUNG, 0) AS DAPANDUNG
    FROM ID_LIST I
    JOIN CAUHOILYTHUYET CH ON CH.CAUHOIID = I.CAUHOIID
    JOIN DAPAN D ON D.CAUHOIID = CH.CAUHOIID
    ORDER BY I.CAUHOIID, D.THUTU;
END;
/
/* =========================================================
   THI MÔ PHỎNG - OBJECT ORACLE
   Dùng cho:
   - DanhSachBoDe
   - LichSuBaiLam
   - LamBai
   - LuuKetQua
   - KetQua
   - LamBaiNgauNhien
   - LuuKetQuaNgauNhien
========================================================= */

------------------------------------------------------------
-- 1. TYPE PHỤ TRỢ CHO ĐỀ NGẪU NHIÊN
------------------------------------------------------------
CREATE OR REPLACE TYPE OBJ_MP_RANDOM_ROW AS OBJECT
(
    IDTHMP          NUMBER,
    TIEUDE          NVARCHAR2(255),
    VIDEOURL        NVARCHAR2(1000),
    SCORESTARTSEC   NUMBER,
    SCOREENDSEC     NUMBER,
    HINTIMAGEURL    NVARCHAR2(500),
    KHO             NUMBER
);
/
CREATE OR REPLACE TYPE TAB_MP_RANDOM_ROW AS TABLE OF OBJ_MP_RANDOM_ROW;
/

------------------------------------------------------------
-- 2. VIEW
------------------------------------------------------------
CREATE OR REPLACE VIEW VW_MP_BODE_TINHHUONG AS
SELECT
    CAST(B.BODEMOPHONGID AS NUMBER(10)) AS BODEMOPHONGID,
    CAST(NVL(B.TENBODE, '') AS NVARCHAR2(255)) AS TENBODE,
    CAST(NVL(B.SOTINHHUONG, 10) AS NUMBER(10)) AS SOTINHHUONG,
    CAST(NVL(B.ISACTIVE, 0) AS NUMBER(1)) AS ISACTIVE,
    CAST(NVL(CT.THUTU, 0) AS NUMBER(10)) AS THUTU,
    CAST(TH.TINHHUONGMOPHONGID AS NUMBER(10)) AS IDTHMP,
    CAST(NVL(TH.TIEUDE, '') AS NVARCHAR2(255)) AS TIEUDE,
    CAST(NVL(TH.VIDEOURL, '') AS NVARCHAR2(1000)) AS VIDEOURL,
    CAST(NVL(TH.URLANHMEO, '') AS NVARCHAR2(500)) AS URLANHMEO,
    CAST(NVL(TH.KHO, 0) AS NUMBER(1)) AS KHO,
    CAST(NVL(TH.TGBATDAU, 0) AS NUMBER(12,4)) AS TGBATDAU,
    CAST(NVL(TH.TGKETTHUC, 0) AS NUMBER(12,4)) AS TGKETTHUC,
    CAST(TH.CHUONGMOPHONGID AS NUMBER(10)) AS CHUONGMOPHONGID
FROM BODEMOPHONG B
JOIN CHITIETBODEMOPHONG CT
    ON CT.BODEMOPHONGID = B.BODEMOPHONGID
JOIN TINHHUONGMOPHONG TH
    ON TH.TINHHUONGMOPHONGID = CT.TINHHUONGMOPHONGID;
/

CREATE OR REPLACE VIEW VW_MP_BAILAM_CHITIET AS
SELECT
    BL.BAILAMMOPHONGID,
    BL.USERID,
    BL.BODEMOPHONGID,
    BL.TONGDIEM,
    BL.KETQUA,
    D.TINHHUONGMOPHONGID AS IDTHMP,
    D.THOIDIEMNGUOIDUNGNHAN,
    TH.TIEUDE,
    TH.VIDEOURL,
    TH.URLANHMEO,
    TH.KHO,
    TH.TGBATDAU,
    TH.TGKETTHUC
FROM BAILAMMOPHONG BL
LEFT JOIN DIEMTUNGTINHHUONG D
    ON D.BAILAMMOPHONGID = BL.BAILAMMOPHONGID
LEFT JOIN TINHHUONGMOPHONG TH
    ON TH.TINHHUONGMOPHONGID = D.TINHHUONGMOPHONGID;
/

------------------------------------------------------------
-- 3. FUNCTION
------------------------------------------------------------
CREATE OR REPLACE FUNCTION FUNC_MP_FRAME_TO_SEC
(
    P_FRAME IN NUMBER
)
RETURN NUMBER
AS
BEGIN
    RETURN NVL(P_FRAME, 0) / 60;
END;
/

CREATE OR REPLACE FUNCTION FUNC_MP_TINH_DIEM
(
    P_TIMEPRESSSEC IN NUMBER,
    P_STARTFRAME   IN NUMBER,
    P_ENDFRAME     IN NUMBER
)
RETURN NUMBER
AS
    V_STARTSEC NUMBER;
    V_ENDSEC   NUMBER;
    V_DURATION NUMBER;
    V_STEP     NUMBER;
    V_INDEX    NUMBER;
BEGIN
    V_STARTSEC := FUNC_MP_FRAME_TO_SEC(P_STARTFRAME);
    V_ENDSEC   := FUNC_MP_FRAME_TO_SEC(P_ENDFRAME);

    IF V_ENDSEC <= V_STARTSEC THEN
        RETURN 0;
    END IF;

    IF NVL(P_TIMEPRESSSEC, 0) < V_STARTSEC OR NVL(P_TIMEPRESSSEC, 0) > V_ENDSEC THEN
        RETURN 0;
    END IF;

    V_DURATION := V_ENDSEC - V_STARTSEC;
    V_STEP     := V_DURATION / 5;

    V_INDEX := FLOOR((P_TIMEPRESSSEC - V_STARTSEC) / V_STEP);

    IF V_INDEX < 0 THEN
        V_INDEX := 0;
    ELSIF V_INDEX > 4 THEN
        V_INDEX := 4;
    END IF;

    RETURN 5 - V_INDEX;
END;
/

CREATE OR REPLACE FUNCTION FUNC_MP_GET_FLAG_TIME
(
    P_FLAGS_TEXT IN CLOB,
    P_IDTHMP     IN NUMBER
)
RETURN NUMBER
AS
    V_TOKEN   VARCHAR2(4000);
    V_ID      NUMBER;
    V_TIME    NUMBER;
    V_IDX     NUMBER := 1;
    V_MIN     NUMBER := NULL;
BEGIN
    LOOP
        V_TOKEN := REGEXP_SUBSTR(P_FLAGS_TEXT, '[^;]+', 1, V_IDX);
        EXIT WHEN V_TOKEN IS NULL;

        V_ID := TO_NUMBER(REGEXP_SUBSTR(V_TOKEN, '^[^|]+'));

        IF V_ID = P_IDTHMP THEN
            V_TIME := TO_NUMBER(
                REGEXP_SUBSTR(V_TOKEN, '[^|]+', 1, 2),
                '9999999990D999999',
                'NLS_NUMERIC_CHARACTERS = ''.,'''
            );

            IF V_MIN IS NULL OR V_TIME < V_MIN THEN
                V_MIN := V_TIME;
            END IF;
        END IF;

        V_IDX := V_IDX + 1;
    END LOOP;

    RETURN V_MIN;
EXCEPTION
    WHEN OTHERS THEN
        RETURN NULL;
END;
/

------------------------------------------------------------
-- 4. PROCEDURE - DANH SÁCH BỘ ĐỀ
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_DANH_SACH_BODE
(
    P_USERID IN NUMBER,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        WITH LATEST_BAILAM AS
        (
            SELECT *
            FROM
            (
                SELECT
                    BL.*,
                    ROW_NUMBER() OVER
                    (
                        PARTITION BY BL.BODEMOPHONGID
                        ORDER BY BL.BAILAMMOPHONGID DESC
                    ) AS RN
                FROM BAILAMMOPHONG BL
                WHERE BL.USERID = P_USERID
            )
            WHERE RN = 1
        )
        SELECT
            B.BODEMOPHONGID AS IDBODE,
            B.TENBODE,
            NVL(B.SOTINHHUONG, 10) AS SOTINHHUONG,
            CASE
                WHEN LB.BAILAMMOPHONGID IS NULL THEN 0
                ELSE 1
            END AS HASRESULT,
            NVL(LB.TONGDIEM, 0) AS TONGDIEM,
            NVL(LB.KETQUA, 0) AS KETQUA,
            CASE
                WHEN LB.BAILAMMOPHONGID IS NULL THEN 0
                ELSE
                    (
                        SELECT COUNT(*)
                        FROM DIEMTUNGTINHHUONG D
                        JOIN TINHHUONGMOPHONG TH
                            ON TH.TINHHUONGMOPHONGID = D.TINHHUONGMOPHONGID
                        WHERE D.BAILAMMOPHONGID = LB.BAILAMMOPHONGID
                          AND
                          (
                              D.THOIDIEMNGUOIDUNGNHAN < FUNC_MP_FRAME_TO_SEC(TH.TGBATDAU)
                              OR D.THOIDIEMNGUOIDUNGNHAN > FUNC_MP_FRAME_TO_SEC(TH.TGKETTHUC)
                          )
                    )
            END AS SOTINHHUONGSAI,
            LB.BAILAMMOPHONGID AS IDBAILAMMOINHAT
        FROM BODEMOPHONG B
        LEFT JOIN LATEST_BAILAM LB
            ON LB.BODEMOPHONGID = B.BODEMOPHONGID
        WHERE B.ISACTIVE = 1
        ORDER BY B.BODEMOPHONGID;
END;
/

------------------------------------------------------------
-- 5. PROCEDURE - LÀM BÀI / CHI TIẾT BỘ ĐỀ
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_LAY_BODE_CHI_TIET
(
    P_IDBODE IN NUMBER,
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            CAST(V.BODEMOPHONGID AS NUMBER(10)) AS IDBODE,
            CAST(V.IDTHMP AS NUMBER(10)) AS IDTHMP,
            CAST(NVL(V.THUTU, 0) AS NUMBER(10)) AS THUTU,
            CAST(NVL(V.SOTINHHUONG, 10) AS NUMBER(10)) AS SOTINHHUONG,

            CAST(V.TENBODE AS NVARCHAR2(255)) AS TENBODE,
            CAST(NVL(V.TIEUDE, '') AS NVARCHAR2(255)) AS TIEUDE,
            CAST(NVL(V.VIDEOURL, '') AS NVARCHAR2(1000)) AS VIDEOURL,
            CAST(NVL(V.URLANHMEO, '') AS NVARCHAR2(500)) AS HINTIMAGEURL,

            CAST(NVL(V.KHO, 0) AS NUMBER(1)) AS KHO,

            CAST(NVL(V.TGBATDAU, 0) / 60 AS NUMBER(12,4)) AS SCORESTARTSEC,
            CAST(NVL(V.TGKETTHUC, 0) / 60 AS NUMBER(12,4)) AS SCOREENDSEC
        FROM VW_MP_BODE_TINHHUONG V
        WHERE V.BODEMOPHONGID = P_IDBODE
        ORDER BY V.THUTU;
END;
/

------------------------------------------------------------
-- 6. PROCEDURE - LỊCH SỬ BÀI LÀM
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_LICH_SU_HEADER
(
    P_IDBAILAM IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            BL.BAILAMMOPHONGID AS IDBAILAM,
            BL.BODEMOPHONGID AS IDBODE,
            BL.TONGDIEM,
            NVL(BL.KETQUA, 0) AS KETQUA
        FROM BAILAMMOPHONG BL
        WHERE BL.BAILAMMOPHONGID = P_IDBAILAM;
END;
/

CREATE OR REPLACE PROCEDURE PROC_MP_LICH_SU_CHI_TIET
(
    P_IDBAILAM IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            CAST(V.BODEMOPHONGID AS NUMBER(10)) AS IDBODE,
            CAST(NVL(V.THUTU, 0) AS NUMBER(10)) AS THUTU,
            CAST(V.IDTHMP AS NUMBER(10)) AS IDTHMP,

            CAST(NVL(V.TIEUDE, '') AS NVARCHAR2(255)) AS TIEUDE,
            CAST(NVL(V.VIDEOURL, '') AS NVARCHAR2(1000)) AS VIDEOURL,
            CAST(NVL(V.URLANHMEO, '') AS NVARCHAR2(500)) AS HINTIMAGEURL,

            CAST(NVL(V.TGBATDAU, 0) / 60 AS NUMBER(12,4)) AS SCORESTARTSEC,
            CAST(NVL(V.TGKETTHUC, 0) / 60 AS NUMBER(12,4)) AS SCOREENDSEC,
            CAST(NVL(D.THOIDIEMNGUOIDUNGNHAN, 0) AS NUMBER(12,4)) AS TIMESEC
        FROM BAILAMMOPHONG BL
        JOIN VW_MP_BODE_TINHHUONG V
            ON V.BODEMOPHONGID = BL.BODEMOPHONGID
        LEFT JOIN DIEMTUNGTINHHUONG D
            ON D.BAILAMMOPHONGID = BL.BAILAMMOPHONGID
           AND D.TINHHUONGMOPHONGID = V.IDTHMP
        WHERE BL.BAILAMMOPHONGID = P_IDBAILAM
        ORDER BY V.THUTU;
END;
/

------------------------------------------------------------
-- 7. PROCEDURE - KẾT QUẢ
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_KET_QUA_HEADER
(
    P_IDBAILAM IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            BL.BAILAMMOPHONGID AS IDBAILAM,
            BL.TONGDIEM,
            NVL(BL.KETQUA, 0) AS KETQUA
        FROM BAILAMMOPHONG BL
        WHERE BL.BAILAMMOPHONGID = P_IDBAILAM;
END;
/

CREATE OR REPLACE PROCEDURE PROC_MP_KET_QUA_CHI_TIET
(
    P_IDBAILAM IN NUMBER,
    P_CURSOR   OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            CAST(D.TINHHUONGMOPHONGID AS NUMBER(10)) AS IDTHMP,
            CAST(NVL(TH.TIEUDE, '') AS NVARCHAR2(255)) AS TIEUDE,
            CAST(NVL(D.THOIDIEMNGUOIDUNGNHAN, 0) AS NUMBER(12,4)) AS THOIDIEMNHAN,
            CAST(
                FUNC_MP_TINH_DIEM(
                    D.THOIDIEMNGUOIDUNGNHAN,
                    TH.TGBATDAU,
                    TH.TGKETTHUC
                ) AS NUMBER(2)
            ) AS DIEM
        FROM DIEMTUNGTINHHUONG D
        JOIN TINHHUONGMOPHONG TH
            ON TH.TINHHUONGMOPHONGID = D.TINHHUONGMOPHONGID
        WHERE D.BAILAMMOPHONGID = P_IDBAILAM
        ORDER BY D.TINHHUONGMOPHONGID;
END;
/

------------------------------------------------------------
-- 8. PROCEDURE - CHẤM ĐIỂM BỘ ĐỀ CỐ ĐỊNH (KHÔNG LƯU)
-- P_FLAGS_TEXT format: IDTHMP|TIMESEC;IDTHMP|TIMESEC;...
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_CHAM_BODE
(
    P_IDBODE     IN NUMBER,
    P_FLAGS_TEXT IN CLOB,
    O_TONGDIEM   OUT NUMBER,
    O_DAT        OUT NUMBER
)
AS
    V_TONG NUMBER := 0;
    V_TIME NUMBER;
BEGIN
    FOR R IN
    (
        SELECT
            V.IDTHMP,
            V.TGBATDAU,
            V.TGKETTHUC
        FROM VW_MP_BODE_TINHHUONG V
        WHERE V.BODEMOPHONGID = P_IDBODE
        ORDER BY V.THUTU
        FETCH FIRST 10 ROWS ONLY
    )
    LOOP
        V_TIME := FUNC_MP_GET_FLAG_TIME(P_FLAGS_TEXT, R.IDTHMP);
        V_TONG := V_TONG + FUNC_MP_TINH_DIEM(NVL(V_TIME, 0), R.TGBATDAU, R.TGKETTHUC);
    END LOOP;

    O_TONGDIEM := V_TONG;
    O_DAT := CASE WHEN V_TONG >= 35 THEN 1 ELSE 0 END;
END;
/

------------------------------------------------------------
-- 9. PROCEDURE - LƯU KẾT QUẢ BỘ ĐỀ CỐ ĐỊNH
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_LUU_KET_QUA
(
    P_USERID      IN NUMBER,
    P_IDBODE      IN NUMBER,
    P_FLAGS_TEXT  IN CLOB,
    O_TONGDIEM    OUT NUMBER,
    O_DAT         OUT NUMBER,
    O_IDBAILAM    OUT NUMBER
)
AS
    V_TIME NUMBER;
BEGIN
    PROC_MP_CHAM_BODE
    (
        P_IDBODE     => P_IDBODE,
        P_FLAGS_TEXT => P_FLAGS_TEXT,
        O_TONGDIEM   => O_TONGDIEM,
        O_DAT        => O_DAT
    );

    INSERT INTO BAILAMMOPHONG
    (
        TONGDIEM,
        KETQUA,
        USERID,
        BODEMOPHONGID
    )
    VALUES
    (
        O_TONGDIEM,
        O_DAT,
        P_USERID,
        P_IDBODE
    )
    RETURNING BAILAMMOPHONGID INTO O_IDBAILAM;

    FOR R IN
    (
        SELECT
            V.IDTHMP
        FROM VW_MP_BODE_TINHHUONG V
        WHERE V.BODEMOPHONGID = P_IDBODE
        ORDER BY V.THUTU
        FETCH FIRST 10 ROWS ONLY
    )
    LOOP
        V_TIME := FUNC_MP_GET_FLAG_TIME(P_FLAGS_TEXT, R.IDTHMP);

        INSERT INTO DIEMTUNGTINHHUONG
        (
            BAILAMMOPHONGID,
            TINHHUONGMOPHONGID,
            THOIDIEMNGUOIDUNGNHAN
        )
        VALUES
        (
            O_IDBAILAM,
            R.IDTHMP,
            NVL(V_TIME, 0)
        );
    END LOOP;
END;
/

------------------------------------------------------------
-- 10. PROCEDURE - LẤY 10 TÌNH HUỐNG NGẪU NHIÊN
-- Tỷ lệ chương: 2-1-2-1-2-2 theo THUTU của CHUONGMOPHONG
-- Có 1 hoặc 2 câu khó
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_DE_NGAU_NHIEN
(
    P_CURSOR OUT SYS_REFCURSOR
)
AS
    TYPE T_NUM_BY_IDX IS TABLE OF NUMBER INDEX BY PLS_INTEGER;

    V_Q1 NUMBER := 2;
    V_Q2 NUMBER := 1;
    V_Q3 NUMBER := 2;
    V_Q4 NUMBER := 1;
    V_Q5 NUMBER := 2;
    V_Q6 NUMBER := 2;

    V_HARD_COUNT NUMBER;
    V_HARD_NOW   NUMBER := 0;

    V_ROWS TAB_MP_RANDOM_ROW := TAB_MP_RANDOM_ROW();

    FUNCTION FN_HAS_ID(P_ID NUMBER) RETURN BOOLEAN
    AS
    BEGIN
        FOR I IN 1 .. V_ROWS.COUNT LOOP
            IF V_ROWS(I).IDTHMP = P_ID THEN
                RETURN TRUE;
            END IF;
        END LOOP;
        RETURN FALSE;
    END;

    PROCEDURE ADD_ROW
    (
        P_IDTHMP        IN NUMBER,
        P_TIEUDE        IN NVARCHAR2,
        P_VIDEOURL      IN NVARCHAR2,
        P_SCORESTARTSEC IN NUMBER,
        P_SCOREENDSEC   IN NUMBER,
        P_HINTIMAGEURL  IN NVARCHAR2,
        P_KHO           IN NUMBER
    )
    AS
    BEGIN
        IF FN_HAS_ID(P_IDTHMP) THEN
            RETURN;
        END IF;

        V_ROWS.EXTEND;
        V_ROWS(V_ROWS.COUNT) := OBJ_MP_RANDOM_ROW
        (
            P_IDTHMP,
            P_TIEUDE,
            P_VIDEOURL,
            P_SCORESTARTSEC,
            P_SCOREENDSEC,
            P_HINTIMAGEURL,
            P_KHO
        );
    END;

    FUNCTION GET_QUOTA(P_THUTU NUMBER) RETURN NUMBER
    AS
    BEGIN
        CASE P_THUTU
            WHEN 1 THEN RETURN V_Q1;
            WHEN 2 THEN RETURN V_Q2;
            WHEN 3 THEN RETURN V_Q3;
            WHEN 4 THEN RETURN V_Q4;
            WHEN 5 THEN RETURN V_Q5;
            WHEN 6 THEN RETURN V_Q6;
            ELSE RETURN 0;
        END CASE;
    END;

    PROCEDURE SET_QUOTA(P_THUTU NUMBER, P_VAL NUMBER)
    AS
    BEGIN
        CASE P_THUTU
            WHEN 1 THEN V_Q1 := P_VAL;
            WHEN 2 THEN V_Q2 := P_VAL;
            WHEN 3 THEN V_Q3 := P_VAL;
            WHEN 4 THEN V_Q4 := P_VAL;
            WHEN 5 THEN V_Q5 := P_VAL;
            WHEN 6 THEN V_Q6 := P_VAL;
        END CASE;
    END;
BEGIN
    V_HARD_COUNT := CASE WHEN DBMS_RANDOM.VALUE(1, 3) < 2 THEN 1 ELSE 2 END;

    -- B1. Pick hard trước theo quota chương
    FOR R IN
    (
        SELECT
            C.THUTU AS CHUONGTHUTU,
            TH.TINHHUONGMOPHONGID,
            TH.TIEUDE,
            TH.VIDEOURL,
            TH.URLANHMEO,
            TH.KHO,
            FUNC_MP_FRAME_TO_SEC(TH.TGBATDAU) AS SCORESTARTSEC,
            FUNC_MP_FRAME_TO_SEC(TH.TGKETTHUC) AS SCOREENDSEC
        FROM CHUONGMOPHONG C
        JOIN TINHHUONGMOPHONG TH
            ON TH.CHUONGMOPHONGID = C.CHUONGMOPHONGID
        WHERE NVL(TH.KHO, 0) = 1
        ORDER BY DBMS_RANDOM.VALUE
    )
    LOOP
        EXIT WHEN V_HARD_NOW >= V_HARD_COUNT;

        IF GET_QUOTA(R.CHUONGTHUTU) > 0 THEN
            ADD_ROW
            (
                R.TINHHUONGMOPHONGID,
                R.TIEUDE,
                R.VIDEOURL,
                R.SCORESTARTSEC,
                R.SCOREENDSEC,
                R.URLANHMEO,
                NVL(R.KHO, 0)
            );

            SET_QUOTA(R.CHUONGTHUTU, GET_QUOTA(R.CHUONGTHUTU) - 1);
            V_HARD_NOW := V_HARD_NOW + 1;
        END IF;
    END LOOP;

    -- B2. Fill theo quota từng chương, ưu tiên câu thường
    FOR CH IN
    (
        SELECT CHUONGMOPHONGID, THUTU
        FROM CHUONGMOPHONG
        ORDER BY THUTU
    )
    LOOP
        IF GET_QUOTA(CH.THUTU) <= 0 THEN
            CONTINUE;
        END IF;

        FOR R IN
        (
            SELECT
                TH.TINHHUONGMOPHONGID,
                TH.TIEUDE,
                TH.VIDEOURL,
                TH.URLANHMEO,
                TH.KHO,
                FUNC_MP_FRAME_TO_SEC(TH.TGBATDAU) AS SCORESTARTSEC,
                FUNC_MP_FRAME_TO_SEC(TH.TGKETTHUC) AS SCOREENDSEC
            FROM TINHHUONGMOPHONG TH
            WHERE TH.CHUONGMOPHONGID = CH.CHUONGMOPHONGID
              AND NVL(TH.KHO, 0) = 0
            ORDER BY DBMS_RANDOM.VALUE
        )
        LOOP
            EXIT WHEN GET_QUOTA(CH.THUTU) <= 0;

            ADD_ROW
            (
                R.TINHHUONGMOPHONGID,
                R.TIEUDE,
                R.VIDEOURL,
                R.SCORESTARTSEC,
                R.SCOREENDSEC,
                R.URLANHMEO,
                NVL(R.KHO, 0)
            );

            IF FN_HAS_ID(R.TINHHUONGMOPHONGID) THEN
                SET_QUOTA(CH.THUTU, GET_QUOTA(CH.THUTU) - 1);
            END IF;
        END LOOP;

        IF GET_QUOTA(CH.THUTU) > 0 THEN
            FOR R IN
            (
                SELECT
                    TH.TINHHUONGMOPHONGID,
                    TH.TIEUDE,
                    TH.VIDEOURL,
                    TH.URLANHMEO,
                    TH.KHO,
                    FUNC_MP_FRAME_TO_SEC(TH.TGBATDAU) AS SCORESTARTSEC,
                    FUNC_MP_FRAME_TO_SEC(TH.TGKETTHUC) AS SCOREENDSEC
                FROM TINHHUONGMOPHONG TH
                WHERE TH.CHUONGMOPHONGID = CH.CHUONGMOPHONGID
                ORDER BY DBMS_RANDOM.VALUE
            )
            LOOP
                EXIT WHEN GET_QUOTA(CH.THUTU) <= 0;

                IF NOT FN_HAS_ID(R.TINHHUONGMOPHONGID) THEN
                    ADD_ROW
                    (
                        R.TINHHUONGMOPHONGID,
                        R.TIEUDE,
                        R.VIDEOURL,
                        R.SCORESTARTSEC,
                        R.SCOREENDSEC,
                        R.URLANHMEO,
                        NVL(R.KHO, 0)
                    );
                    SET_QUOTA(CH.THUTU, GET_QUOTA(CH.THUTU) - 1);
                END IF;
            END LOOP;
        END IF;
    END LOOP;

    -- B3. Nếu thiếu thì fill toàn bộ hệ thống
    IF V_ROWS.COUNT < 10 THEN
        FOR R IN
        (
            SELECT
                TH.TINHHUONGMOPHONGID,
                TH.TIEUDE,
                TH.VIDEOURL,
                TH.URLANHMEO,
                TH.KHO,
                FUNC_MP_FRAME_TO_SEC(TH.TGBATDAU) AS SCORESTARTSEC,
                FUNC_MP_FRAME_TO_SEC(TH.TGKETTHUC) AS SCOREENDSEC
            FROM TINHHUONGMOPHONG TH
            ORDER BY DBMS_RANDOM.VALUE
        )
        LOOP
            EXIT WHEN V_ROWS.COUNT >= 10;

            IF NOT FN_HAS_ID(R.TINHHUONGMOPHONGID) THEN
                ADD_ROW
                (
                    R.TINHHUONGMOPHONGID,
                    R.TIEUDE,
                    R.VIDEOURL,
                    R.SCORESTARTSEC,
                    R.SCOREENDSEC,
                    R.URLANHMEO,
                    NVL(R.KHO, 0)
                );
            END IF;
        END LOOP;
    END IF;

    OPEN P_CURSOR FOR
    SELECT
        CAST(T.IDTHMP AS NUMBER(10)) AS IDTHMP,
        CAST(NVL(T.TIEUDE, '') AS NVARCHAR2(255)) AS TIEUDE,
        CAST(NVL(T.VIDEOURL, '') AS NVARCHAR2(1000)) AS VIDEOURL,
        CAST(NVL(T.SCORESTARTSEC, 0) AS NUMBER(12,4)) AS SCORESTARTSEC,
        CAST(NVL(T.SCOREENDSEC, 0) AS NUMBER(12,4)) AS SCOREENDSEC,
        CAST(NVL(T.HINTIMAGEURL, '') AS NVARCHAR2(500)) AS HINTIMAGEURL,
        CAST(NVL(T.KHO, 0) AS NUMBER(1)) AS KHO
    FROM
    (
        SELECT T.*
        FROM TABLE(V_ROWS) T
        ORDER BY DBMS_RANDOM.VALUE
    ) T
    FETCH FIRST 10 ROWS ONLY;
END;
/

------------------------------------------------------------
-- 11. PROCEDURE - CHẤM ĐỀ NGẪU NHIÊN (KHÔNG LƯU)
-- P_SELECTED_IDS format: 1,5,9,12...
------------------------------------------------------------
CREATE OR REPLACE PROCEDURE PROC_MP_CHAM_NGAU_NHIEN
(
    P_SELECTED_IDS IN CLOB,
    P_FLAGS_TEXT   IN CLOB,
    O_TONGDIEM     OUT NUMBER,
    O_DAT          OUT NUMBER
)
AS
    V_ID_STR VARCHAR2(100);
    V_ID     NUMBER;
    V_TIME   NUMBER;
    V_TONG   NUMBER := 0;
    V_IDX    NUMBER := 1;
    V_START  NUMBER;
    V_END    NUMBER;
BEGIN
    LOOP
        V_ID_STR := REGEXP_SUBSTR(P_SELECTED_IDS, '[^,]+', 1, V_IDX);
        EXIT WHEN V_ID_STR IS NULL;

        V_ID := TO_NUMBER(TRIM(V_ID_STR));
        V_TIME := FUNC_MP_GET_FLAG_TIME(P_FLAGS_TEXT, V_ID);

        SELECT TGBATDAU, TGKETTHUC
        INTO V_START, V_END
        FROM TINHHUONGMOPHONG
        WHERE TINHHUONGMOPHONGID = V_ID;

        V_TONG := V_TONG + FUNC_MP_TINH_DIEM(NVL(V_TIME, 0), V_START, V_END);

        V_IDX := V_IDX + 1;
    END LOOP;

    O_TONGDIEM := V_TONG;
    O_DAT := CASE WHEN V_TONG >= 35 THEN 1 ELSE 0 END;
END;
/
CREATE OR REPLACE PROCEDURE PROC_MP_ONTAP_INDEX
(
    P_CURSOR OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            CAST(C.CHUONGMOPHONGID AS NUMBER(10)) AS IDCHUONGMP,
            CAST(NVL(C.TENCHUONG, '') AS NVARCHAR2(255)) AS TENCHUONG,
            CAST(NVL(C.THUTU, 0) AS NUMBER(10)) AS CHUONG_THUTU,

            CAST(TH.TINHHUONGMOPHONGID AS NUMBER(10)) AS IDTHMP,
            CAST(NVL(TH.TIEUDE, '') AS NVARCHAR2(255)) AS TIEUDE,
            CAST(NVL(TH.VIDEOURL, '') AS NVARCHAR2(1000)) AS VIDEOURL,
            CAST(NVL(TH.URLANHMEO, '') AS NVARCHAR2(500)) AS HINTIMAGEURL,
            CAST(NVL(TH.THUTU, 0) AS NUMBER(10)) AS TINHHUONG_THUTU,
            CAST(NVL(TH.KHO, 0) AS NUMBER(1)) AS KHO,

            CAST(NVL(TH.TGBATDAU, 0) / 60 AS NUMBER(12,4)) AS SCORESTARTSEC,
            CAST(NVL(TH.TGKETTHUC, 0) / 60 AS NUMBER(12,4)) AS SCOREENDSEC
        FROM CHUONGMOPHONG C
        LEFT JOIN TINHHUONGMOPHONG TH
            ON TH.CHUONGMOPHONGID = C.CHUONGMOPHONGID
        ORDER BY C.THUTU, TH.THUTU, TH.TINHHUONGMOPHONGID;
END;
/
-- KHÓA HỌC V2
CREATE OR REPLACE PROCEDURE SP_KHOAHOC_CHECK_DANGKY
(
    p_userId    IN NUMBER,
    p_khoaHocId IN NUMBER,
    p_cursor    OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        WITH KHOA_HOC_TARGET AS
        (
            SELECT
                kh.khoaHocId,
                kh.tenKhoaHoc,
                kh.ngayBatDau,
                kh.ngayKetThuc,
                kh.diaDiem,
                kh.trangThai,
                hg.hangId,
                hg.tenHang,
                hg.moTa,
                hg.loaiPhuongTien,
                hg.soCauHoi,
                hg.diemDat,
                hg.thoiGianTn,
                hg.hocPhi
            FROM KhoaHoc kh
            JOIN HangGplx hg ON hg.hangId = kh.hangId
            WHERE kh.khoaHocId = p_khoaHocId
        ),
        HOC_VIEN_USER AS
        (
            SELECT hv.hocVienId
            FROM HocVien hv
            WHERE hv.userId = p_userId
        ),
        HO_SO_CUNG_HANG AS
        (
            SELECT
                hs.hoSoId,
                hs.tenHoSo,
                hs.ngayDangKy,
                hs.trangThai AS trangThaiHoSo,
                hs.hangId,
                ROW_NUMBER() OVER
                (
                    ORDER BY NVL(hs.ngayDangKy, DATE '1900-01-01') DESC, hs.hoSoId DESC
                ) AS rn
            FROM HoSoThiSinh hs
            JOIN HOC_VIEN_USER hvu ON hvu.hocVienId = hs.hocVienId
            JOIN KHOA_HOC_TARGET kht ON kht.hangId = hs.hangId
        ),
        HO_SO_PHU_HOP_DA_DUYET AS
        (
            SELECT *
            FROM
            (
                SELECT
                    hs.hoSoId,
                    hs.tenHoSo,
                    hs.ngayDangKy,
                    hs.trangThaiHoSo,
                    hs.hangId,
                    ROW_NUMBER() OVER
                    (
                        ORDER BY NVL(hs.ngayDangKy, DATE '1900-01-01') DESC, hs.hoSoId DESC
                    ) AS rn2
                FROM HO_SO_CUNG_HANG hs
                WHERE hs.trangThaiHoSo = N'Đã duyệt'
            )
            WHERE rn2 = 1
        ),
        HO_SO_CHUA_DUYET AS
        (
            SELECT COUNT(*) AS soHoSoChuaDuyet
            FROM HO_SO_CUNG_HANG
            WHERE trangThaiHoSo <> N'Đã duyệt' OR trangThaiHoSo IS NULL
        ),
        DA_HOC_HANG AS
        (
            SELECT COUNT(*) AS soLanDaHoc
            FROM KetQuaHocTap kq
            JOIN HoSoThiSinh hs ON hs.hoSoId = kq.hoSoId
            JOIN HOC_VIEN_USER hvu ON hvu.hocVienId = hs.hocVienId
            JOIN ChiTietKetQuaHocTap ct ON ct.ketQuaHocTapId = kq.ketQuaHocTapId
            JOIN KhoaHoc kh ON kh.khoaHocId = ct.khoaHocId
            JOIN KHOA_HOC_TARGET kht ON kht.hangId = kh.hangId
        )
        SELECT
            kht.khoaHocId,
            kht.tenKhoaHoc,
            kht.ngayBatDau,
            kht.ngayKetThuc,
            kht.diaDiem,
            kht.trangThai,
            kht.hangId,
            kht.tenHang,
            kht.moTa,
            kht.loaiPhuongTien,
            kht.soCauHoi,
            kht.diemDat,
            kht.thoiGianTn,
            kht.hocPhi,

            CASE
                WHEN kht.trangThai = N'Sắp khai giảng' THEN 1
                ELSE 0
            END AS isMoDangKy,

            CASE
                WHEN hsp.hoSoId IS NOT NULL THEN 1
                ELSE 0
            END AS hasHoSoPhuHop,

            CASE
                WHEN NVL(hscd.soHoSoChuaDuyet, 0) > 0 AND hsp.hoSoId IS NULL THEN 1
                ELSE 0
            END AS hasHoSoChuaDuyet,

            NVL(hsp.hoSoId, 0) AS hoSoIdPhuHop,
            NVL(hsp.tenHoSo, N'') AS tenHoSoPhuHop,
            hsp.ngayDangKy AS ngayDangKyHoSo,
            NVL(hsp.trangThaiHoSo, N'') AS trangThaiHoSo,

            CASE
                WHEN NVL(dhh.soLanDaHoc, 0) > 0 THEN 1
                ELSE 0
            END AS daTungHocHang,

            NVL(dhh.soLanDaHoc, 0) AS soLanDaHocHang
        FROM KHOA_HOC_TARGET kht
        LEFT JOIN HO_SO_PHU_HOP_DA_DUYET hsp ON 1 = 1
        LEFT JOIN HO_SO_CHUA_DUYET hscd ON 1 = 1
        LEFT JOIN DA_HOC_HANG dhh ON 1 = 1;
END;
/







COMMIT;