
import { useState } from "react";
import { Alert, Button, Grid, TextField, Typography } from "@mui/material";
import { useHistory } from "react-router-dom";
import api from "services/axios";

// Vision UI Dashboard React components
import VuiBox from "components/VuiBox";

// Vision UI Dashboard React example components
import DashboardLayout from "examples/LayoutContainers/DashboardLayout";
import DashboardNavbar from "examples/Navbars/DashboardNavbar";
import Footer from "examples/Footer";

function newRFP() {
  const history = useHistory();
  const [formValues, setFormValues] = useState({
    clientName: "",
    clientEmail: "",
    clientRequirement: "",
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const handleChange = (field) => (event) => {
    setFormValues((currentValues) => ({
      ...currentValues,
      [field]: event.target.value,
    }));
    setError("");
    setSuccess("");
  };

  const handleGenerate = async () => {
    setIsLoading(true);
    setError("");
    setSuccess("");

    try {
      const result = await api.post("/api/Rfp/RFP_Generation", formValues);
      const rfpId = result?.id || result?.rfpId || result?.data?.id;

      if (!rfpId) {
        throw new Error("The RFP was generated, but no RFP ID was returned.");
      }

      localStorage.setItem("rfpPrompt", result?.rfpPrompt || result?.generatedPrompt || formValues.clientRequirement);
      localStorage.setItem("rfpResponse", JSON.stringify(result));
      setSuccess("RFP generated successfully.");
      history.push(`/rfp/${rfpId}`);
    } catch (requestError) {
      setError(
        requestError.response?.data?.message ||
          requestError.message ||
          "Unable to generate the RFP. Please try again."
      );
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <DashboardLayout>
      <DashboardNavbar />
      <VuiBox mt={4}>
        <VuiBox my={3}>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Typography
                variant="h5"
                sx={{ color: "#d8eaff", fontSize: "1.35rem", fontWeight: 500 }}
                gutterBottom
              >
                Requirement for Proposal
              </Typography>
              <Typography variant="body2" sx={{ color: "rgba(181, 211, 244, 0.78)", fontSize: "0.85rem" }} mb={2}>
                Describe what you need and generate a RFP.
              </Typography>
              <Grid container spacing={2} mb={2}>
                <Grid item xs={12} md={6}>
                  <TextField
                    fullWidth
                    size="small"
                    label="Client name"
                    value={formValues.clientName}
                    onChange={handleChange("clientName")}
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <TextField
                    fullWidth
                    size="small"
                    label="Client email"
                    type="email"
                    value={formValues.clientEmail}
                    onChange={handleChange("clientEmail")}
                  />
                </Grid>
              </Grid>
              <TextField
                fullWidth
                multiline
                minRows={8}
                // label="RFP prompt"
                value={formValues.clientRequirement}
                onChange={handleChange("clientRequirement")}
                placeholder="Enter your RFP prompt"
                sx={{
                  "& .MuiInputBase-root": {
                    width: "100%",
                    boxSizing: "border-box",
                    fontSize: "0.875rem",
                    color: "#ffffff",
                    backgroundColor: "rgba(6, 11, 40, 0.28) !important",
                    backdropFilter: "blur(14px)",
                    borderRadius: "14px",
                    boxShadow: "inset 0 1px 0 rgba(255, 255, 255, 0.08)",
                    transition: "background-color 180ms ease, box-shadow 180ms ease",
                    "& fieldset": {
                      borderColor: "rgba(160, 174, 192, 0.35)",
                      transition: "border-color 180ms ease",
                    },
                    "&:hover": {
                      backgroundColor: "rgba(6, 11, 40, 0.42) !important",
                    },
                    "&:hover fieldset": {
                      borderColor: "rgba(57, 147, 254, 0.75)",
                    },
                    "&.Mui-focused fieldset": {
                      borderColor: "#3993fe",
                    },
                    "&.Mui-focused": {
                      backgroundColor: "rgba(6, 11, 40, 0.48) !important",
                      boxShadow: "0 0 0 3px rgba(57, 147, 254, 0.14)",
                    },
                  },
                  "& .MuiInputBase-input": {
                    width: "100%",
                    minWidth: 0,
                    boxSizing: "border-box",
                    fontSize: "0.875rem",
                    color: "#ffffff !important",
                    "&::selection": {
                      backgroundColor: "rgba(57, 147, 254, 0.45)",
                    },
                  },
                  "& .MuiInputBase-multiline": {
                    display: "flex",
                    alignItems: "stretch",
                    width: "100%",
                  },
                  "& .MuiInputBase-inputMultiline": {
                    display: "block",
                    flex: "1 1 auto",
                    width: "100%",
                    minWidth: 0,
                    boxSizing: "border-box",
                    whiteSpace: "pre-wrap",
                    overflowWrap: "break-word",
                  },
                  "& .MuiInputLabel-root": {
                    color: "#ffffff",
                  },
                  "& .MuiInputLabel-root.Mui-focused": {
                    color: "#ffffff",
                  },
                  "& textarea::placeholder": {
                    color: "rgba(181, 211, 244, 0.72)",
                    opacity: 1,
                  },
                }}
              />
              <Button
                size="small"
                variant="contained"
                color="info"
                onClick={handleGenerate}
                disabled={
                  isLoading ||
                  !formValues.clientName.trim() ||
                  !formValues.clientEmail.trim() ||
                  !formValues.clientRequirement.trim()
                }
                sx={{
                  mt: 2,
                  px: 2.5,
                  color: "#0b0a0a",
                  "&.Mui-disabled": {
                    color: "rgba(255, 255, 255, 0.6)",
                  },
                }}
              >
                {isLoading ? "Generating..." : "Generate"}
              </Button>
              {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
              {success && <Alert severity="success" sx={{ mt: 2 }}>{success}</Alert>}
              {/* {generatedPrompt && (
                <Box
                  mt={3}
                  p={2}
                  sx={{
                    border: "1px solid #56577a",
                    borderRadius: 2,
                    backgroundColor: "rgba(6, 11, 40, 0.5)",
                  }}
                >
                  <Typography variant="caption" color="text">
                    Generated prompt
                  </Typography>
                  <Typography color="white" sx={{ whiteSpace: "pre-wrap", mt: 1 }}>
                    {generatedPrompt}
                  </Typography>
                </Box>
              )} */}
            </Grid>
          </Grid>
        </VuiBox>
      </VuiBox>
      {/* <Footer /> */}
    </DashboardLayout>
  );
}

export default newRFP;
