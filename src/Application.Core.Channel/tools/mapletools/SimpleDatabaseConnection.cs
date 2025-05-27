

using org.apache.logging.log4j;
using org.apache.logging.log4j;
using org.apache.logging.log4j.core.config;
using tools;

namespace tools.mapletools;




class SimpleDatabaseConnection {
    private SimpleDatabaseConnection() {}

    static Connection getConnection() {
        muffleLogging();
        DatabaseConnection.initializeConnectionPool();

        try {
            return DatabaseConnection.getConnection();
        } catch (SQLException e) {
            throw new IllegalStateException("Failed to get database connection", e);
        }
    }

    private static void muffleLogging() {
        Level minimumVisibleLevel = Level.WARN;
        Configurator.setLevel(LogManager.getLogger(com.zaxxer.hikari.HikariDataSource.class).getName(), minimumVisibleLevel);
        Configurator.setRootLevel(minimumVisibleLevel);
    }
}
