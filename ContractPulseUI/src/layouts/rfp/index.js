
import { useEffect, useState } from "react";
import { Alert, Box, Button, Grid, IconButton, TextField, Typography } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import { useHistory, useParams } from "react-router-dom";
import api from "services/axios";

// Vision UI Dashboard React components
import VuiBox from "components/VuiBox";

// Vision UI Dashboard React example components
import DashboardLayout from "examples/LayoutContainers/DashboardLayout";
import DashboardNavbar from "examples/Navbars/DashboardNavbar";
import Footer from "examples/Footer";

function RFP() {
  const history = useHistory();
  const { id } = useParams();
  const [prompt, setPrompt] = useState("");
  const [isEditing, setIsEditing] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchRfp = async () => {
      if (!id) {
        setError("RFP ID is missing from the URL.");
        setIsLoading(false);
        return;
      }

      try {
        const rfp = await api.get(`/api/Rfp/${id}`);
        const rfpPrompt = rfp?.rfp_prompt;

        if (!rfpPrompt) {
          throw new Error("RFP prompt was not returned by the server.");
        }

        setPrompt(rfpPrompt);
        localStorage.setItem("rfpPrompt", rfpPrompt);
      } catch (requestError) {
        setError(
          requestError.response?.data?.message ||
            requestError.message ||
            "Unable to load the RFP."
        );
      } finally {
        setIsLoading(false);
      }
    };

    fetchRfp();
  }, [id]);

  const handlNext = () => {
    history.push(`/sow/${id}`);
  };

  const handleSave = () => {
    localStorage.setItem("rfpPrompt", prompt);
    setIsEditing(false);
  };

  return (
    <DashboardLayout>
      <DashboardNavbar />
      <VuiBox mt={4}>
        <VuiBox my={3}>
          <Grid container spacing={3} data-rfp-id={id}>
            <Grid item xs={12}>
              <Typography
                variant="h5"
                sx={{ color: "#d8eaff", fontSize: "1.35rem", fontWeight: 500 }}
                gutterBottom
              >
                RFP Generated
              </Typography>
              <Typography variant="body2" sx={{ color: "rgba(181, 211, 244, 0.78)", fontSize: "0.85rem" }} mb={2}>
                Review the generated RFP based on your prompt below. You can edit the prompt and generate a new RFP if needed.
              </Typography>
              <Box display="flex" justifyContent="flex-end" mb={1}>
                <IconButton
                  onClick={() => setIsEditing(true)}
                  disabled={isEditing}
                  aria-label="Edit RFP prompt"
                  sx={{
                    color: "#a9cdf2",
                    p: 0,
                  }}
                >
                  <EditIcon />
                </IconButton>
              </Box>
              <TextField
                fullWidth
                multiline
                minRows={8}
                value={isLoading ? "Loading RFP..." : prompt}
                onChange={(event) => setPrompt(event.target.value)}
                InputProps={{ readOnly: !isEditing || isLoading }}
                placeholder="This is a sample RFP generated"
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
                color="success"
                onClick={handleSave}
                disabled={!isEditing || isLoading}
                sx={{
                  mt: 2,
                  mr: 1,
                  px: 2.5,
                  color: "#0b0a0a",
                  "&.Mui-disabled": {
                    color: "rgba(255, 255, 255, 0.6)",
                   },
                }}
              >
                Save
              </Button>
              <Button
                size="small"
                variant="contained"
                color="info"
                onClick={handleNext}
                disabled={isEditing || isLoading || !prompt.trim()}
                sx={{
                  mt: 2,
                  px: 2.5,
                  color: "#0b0a0a",
                  "&.Mui-disabled": {
                    color: "rgba(255, 255, 255, 0.6)",
                   },
                }}
              >
                Next
              </Button>
              {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
             
            </Grid>
          </Grid>
        </VuiBox>
      </VuiBox>
      {/* <Footer /> */}
    </DashboardLayout>
  );
}

export default RFP;
